using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs;

public class CqrsEndpointDescriptorRegistrationTests
{
    [Fact]
    public async Task MapQueryAndPostCommand_AttachDescriptorWithCorrectHttpMethodAsync()
    {
        // Arrange — SingleQuery has nullable AppId/StringId but is registered WITHOUT nullable-route expansion.
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        app.MapQuery<CqrsEndpointDescriptorBuilderTests.SingleQuery>("apps/{appId}/strings/{stringId:int}/value");
        app.MapPostCommand<CqrsEndpointDescriptorBuilderTests.CreateCommand>("items");

        await app.StartAsync();
        try
        {
            var dataSource = app.Services.GetRequiredService<EndpointDataSource>();

            // Assert — the query endpoint carries a GET descriptor with no advertised nullable route parameters.
            var queryDescriptor = SingleDescriptor(dataSource, typeof(CqrsEndpointDescriptorBuilderTests.SingleQuery));
            Assert.Equal("GET", queryDescriptor.HttpMethod);
            Assert.True(queryDescriptor.IsQuery);
            Assert.Equal(ResponseShape.Item, queryDescriptor.ResponseShape);
            Assert.Empty(queryDescriptor.NullableRouteParameters);

            // Assert — the POST command carries a POST descriptor with a body payload.
            var commandDescriptor = SingleDescriptor(
                dataSource,
                typeof(CqrsEndpointDescriptorBuilderTests.CreateCommand));
            Assert.Equal("POST", commandDescriptor.HttpMethod);
            Assert.False(commandDescriptor.IsQuery);
            Assert.Equal(typeof(CqrsEndpointDescriptorBuilderTests.CreateCommand), commandDescriptor.PayloadType);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task MapPutAndDeleteCommands_AttachDescriptorWithCorrectHttpMethodAsync()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        app.MapPutCommand<CqrsEndpointDescriptorBuilderTests.UpdateCommand>("items/{id:int}");
        app.MapDeleteCommand<CqrsEndpointDescriptorBuilderTests.DeleteCommand>("items/{id:int}");

        await app.StartAsync();
        try
        {
            var dataSource = app.Services.GetRequiredService<EndpointDataSource>();

            // Assert
            var putDescriptor = SingleDescriptor(dataSource, typeof(CqrsEndpointDescriptorBuilderTests.UpdateCommand));
            Assert.Equal("PUT", putDescriptor.HttpMethod);

            var deleteDescriptor = SingleDescriptor(
                dataSource,
                typeof(CqrsEndpointDescriptorBuilderTests.DeleteCommand));
            Assert.Equal("DELETE", deleteDescriptor.HttpMethod);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task MapQueryWithNullableRouteExpansion_PopulatesDescriptorAndSharesInstanceAsync()
    {
        // Arrange — SingleQuery has two nullable route parameters (AppId, StringId), so enabling nullable
        // expansion yields 2^2 = 4 routes, all sharing one descriptor instance.
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        app.MapQuery<CqrsEndpointDescriptorBuilderTests.SingleQuery>(
            "apps/{appId}/strings/{stringId:int}/value",
            MapNullableRouteParameter.Enable);

        await app.StartAsync();
        try
        {
            var dataSource = app.Services.GetRequiredService<EndpointDataSource>();

            // Assert — exactly 4 expanded routes, each a distinct concrete template.
            var endpoints = dataSource.Endpoints.OfType<RouteEndpoint>()
                .Where(e => e.Metadata.GetMetadata<CqrsEndpointDescriptor>() is { RequestType: { } rt }
                            && rt == typeof(CqrsEndpointDescriptorBuilderTests.SingleQuery))
                .ToList();
            Assert.Equal(4, endpoints.Count);
            Assert.Equal(4, endpoints.Select(e => e.RoutePattern.RawText).Distinct().Count());

            // The descriptor is built once and shared across all expanded endpoints (reference identity), and it
            // advertises the expanded nullable route parameters.
            var distinctDescriptors = endpoints
                .Select(e => e.Metadata.GetMetadata<CqrsEndpointDescriptor>())
                .Distinct()
                .ToList();
            Assert.Single(distinctDescriptors);
            Assert.Equivalent(new[] { "AppId", "StringId" }, distinctDescriptors[0]!.NullableRouteParameters);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static CqrsEndpointDescriptor SingleDescriptor(EndpointDataSource dataSource, Type requestType)
    {
        var matches = dataSource.Endpoints.OfType<RouteEndpoint>()
            .Select(e => e.Metadata.GetMetadata<CqrsEndpointDescriptor>())
            .Where(d => d is { RequestType: { } rt } && rt == requestType)
            .ToList();
        Assert.True(
            matches.Count == 1,
            $"Expected exactly one descriptor for {requestType.Name}, found {matches.Count}.");
        return matches[0]!;
    }
}
