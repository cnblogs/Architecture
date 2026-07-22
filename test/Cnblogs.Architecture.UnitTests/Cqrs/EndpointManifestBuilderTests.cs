using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.ServiceAgent.Design;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs;

public class EndpointManifestBuilderTests
{
    public record SampleDto(int Id, string Title);

    public record SingleQuery(string? AppId = null, int? StringId = null, bool Found = true)
        : IQuery<string>;

    public record CreateCommand : ICommand<string, TestError>
    {
        public bool ValidateOnly => false;
    }

    public record UpdateCommand : ICommand<TestError>
    {
        public bool ValidateOnly => false;
    }

    public record DeleteCommand(int Id, bool NeedError, bool ValidateOnly = false)
        : ICommand<string, TestError>;

    public record OtherSegmentCommand : ICommand<string, TestError>
    {
        public bool ValidateOnly => false;
    }

    public class TestError : Enumeration
    {
        public static readonly TestError None = new(0, nameof(None));

        public TestError(int id, string name)
            : base(id, name)
        {
        }
    }

    public class TestError2 : Enumeration
    {
        public static readonly TestError2 None = new(0, nameof(None));

        public TestError2(int id, string name)
            : base(id, name)
        {
        }
    }

    public record MixedCommand1 : ICommand<string, TestError>
    {
        public bool ValidateOnly => false;
    }

    public record MixedCommand2 : ICommand<string, TestError2>
    {
        public bool ValidateOnly => false;
    }

    // ===== Helpers =====

    private static async Task<EndpointManifest> BuildManifestAsync(
        Action<WebApplication> mapEndpoints)
    {
        var builder = WebApplication.CreateBuilder();
        // Bind an ephemeral port so parallel WebApplication-based test classes don't fight for the default port.
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        mapEndpoints(app);

        await app.StartAsync();
        try
        {
            var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
            return EndpointManifestBuilder.Build(dataSource);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static ManifestEndpoint SingleEndpoint(EndpointManifest manifest, string requestTypeName)
    {
        var matches = manifest.Groups
            .SelectMany(g => g.Endpoints)
            .Where(e => e.RequestTypeName == requestTypeName)
            .ToList();
        Assert.True(
            matches.Count == 1,
            $"Expected exactly one endpoint with request {requestTypeName}, found {matches.Count}.");
        return matches[0];
    }

    private static ManifestParameter SingleParameter(ManifestEndpoint endpoint, string name)
    {
        var matches = endpoint.Parameters.Where(p => p.Name == name).ToList();
        Assert.True(
            matches.Count == 1,
            $"Expected exactly one parameter named {name}, found {matches.Count}.");
        return matches[0];
    }

    // ===== Tests =====

    [Fact]
    public async Task Build_SingleGetQuery_ExpandsParametersAndCarriesRouteAsync()
    {
        // Act
        var manifest = await BuildManifestAsync(
            app => app.MapQuery<SingleQuery>("apps/{appId}/strings/{stringId:int}/value"));

        // Assert
        var endpoint = SingleEndpoint(manifest, nameof(SingleQuery));
        Assert.Equal("GET", endpoint.HttpMethod);
        Assert.Contains("GET", endpoint.HttpMethods);
        Assert.Equal("apps/{appId}/strings/{stringId:int}/value", endpoint.Route);
        Assert.True(endpoint.IsQuery);
        Assert.Equal(ResponseShape.Item, endpoint.ResponseShape);
        Assert.Equal("String", endpoint.ResponseType?.Name);
        Assert.Empty(endpoint.NullableRouteParameters);

        var appId = SingleParameter(endpoint, "AppId");
        Assert.Equal(ParameterSource.Route, appId.Source);
        Assert.Equal("appId", appId.RouteToken);
        Assert.Equal("String", appId.ClrType.Name);
        Assert.True(appId.IsNullable);

        var stringId = SingleParameter(endpoint, "StringId");
        Assert.Equal(ParameterSource.Route, stringId.Source);
        Assert.Equal("stringId", stringId.RouteToken);
        Assert.Equal("Int32", stringId.ClrType.Name);
        Assert.True(stringId.IsNullable);

        var found = SingleParameter(endpoint, "Found");
        Assert.Equal(ParameterSource.Query, found.Source);
        Assert.Equal("Boolean", found.ClrType.Name);
        Assert.Null(found.RouteToken);
    }

    [Fact]
    public async Task Build_PostCommand_AttachesBodyPayloadAndErrorTypeAsync()
    {
        // Act
        var manifest = await BuildManifestAsync(
            app => app.MapPostCommand<CreateCommand>("items"));

        // Assert
        var endpoint = SingleEndpoint(manifest, nameof(CreateCommand));
        Assert.Equal("POST", endpoint.HttpMethod);
        Assert.False(endpoint.IsQuery);
        Assert.Equal("items", endpoint.Route);
        Assert.Equal(ResponseShape.Item, endpoint.ResponseShape);
        Assert.Equal("String", endpoint.ResponseType?.Name);
        Assert.EndsWith(nameof(CreateCommand), endpoint.PayloadType?.Name);

        var group = Assert.Single(manifest.Groups);
        Assert.NotNull(group.ErrorType);
        Assert.EndsWith(nameof(TestError), group.ErrorType!.Name);
    }

    [Fact]
    public async Task Build_CommandsSharingSegment_JoinOneGroupByErrorSuffixAsync()
    {
        // Act — three commands under the "items" segment all use TestError, plus a query on the same segment.
        var manifest = await BuildManifestAsync(
            app =>
            {
                app.MapPostCommand<CreateCommand>("items");
                app.MapDeleteCommand<DeleteCommand>("items/{id:int}");
                app.MapQuery<SingleQuery>("items/{appId}/strings/{stringId:int}/value");
            });

        // Assert — all four endpoints land in a single group named "Test" (TestError minus the "Error" suffix).
        var group = Assert.Single(manifest.Groups);
        Assert.Equal("Test", group.Name);
        Assert.NotNull(group.ErrorType);
        Assert.EndsWith(nameof(TestError), group.ErrorType!.Name);
        Assert.True(group.Endpoints.Count >= 3);
    }

    [Fact]
    public async Task Build_QueryWithoutMatchingCommandSegment_GetsOwnSegmentGroupWithNullErrorAsync()
    {
        // Act — a query on a segment with no commands.
        var manifest = await BuildManifestAsync(
            app => app.MapQuery<SingleQuery>("metrics/{appId}/totals"));

        // Assert — the group is named after the first segment, and it has no error type.
        var group = Assert.Single(manifest.Groups);
        Assert.Equal("Metrics", group.Name);
        Assert.Null(group.ErrorType);
        Assert.Single(group.Endpoints);
    }

    [Fact]
    public async Task Build_ExplicitServiceAgentGroup_WinsOverSegmentInferenceAsync()
    {
        // Act — tag the route group with an explicit name.
        var manifest = await BuildManifestAsync(
            app =>
            {
                var group = app.MapGroup("api/items");
                group.WithServiceAgentGroup("Custom");
                group.MapQuery<SingleQuery>("apps/{appId}/strings/{stringId:int}/value");
                group.MapPostCommand<CreateCommand>("new");
            });

        // Assert
        var group0 = Assert.Single(manifest.Groups);
        Assert.Equal("Custom", group0.Name);
        Assert.Equal(2, group0.Endpoints.Count);
    }

    [Fact]
    public async Task Build_MixedErrorTypesInOneExplicitGroup_ThrowsInvalidOperationExceptionAsync()
    {
        // Act — two commands with different error types, both tagged with the same explicit group name.
        Task<EndpointManifest> Act()
        {
            return BuildManifestAsync(
                app =>
                {
                    var group = app.MapGroup("api/items");
                    group.WithServiceAgentGroup("Mixed");
                    group.MapPostCommand<MixedCommand1>("a");
                    group.MapPostCommand<MixedCommand2>("b");
                });
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }
}
