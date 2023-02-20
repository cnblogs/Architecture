using System.Net;
using Cnblogs.Architecture.IntegrationTestProject.EventHandlers;
using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.IntegrationTests;

public class DaprTests
{
    [Theory]
    [InlineData(SubscribeType.ByEvent)]
    [InlineData(SubscribeType.ByEventAssemblies)]
    [InlineData(SubscribeType.ByEventHandler)]
    [InlineData(SubscribeType.ByEventHandlerAssemblies)]
    public async Task Dapr_SubscribeEndpoint_OkAsync(SubscribeType subscribeType)
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDaprEventBus(nameof(DaprTests));
        builder.WebHost.UseTestServer();

        using var app = builder.Build();

        _ = subscribeType switch
        {
            SubscribeType.ByEvent => app.Subscribe<TestIntegrationEvent>().Subscribe<BlogPostCreatedIntegrationEvent>(),
            SubscribeType.ByEventAssemblies => app.Subscribe(typeof(TestIntegrationEvent).Assembly),
            SubscribeType.ByEventHandler => app.SubscribeByEventHandler<TestIntegrationEventHandler>(),
            SubscribeType.ByEventHandlerAssemblies => app.SubscribeByEventHandler(typeof(TestIntegrationEventHandler).Assembly),
            _ => app
        };

        await app.StartAsync();
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.GetAsync("/dapr/subscribe");

        // Assert
        response.Should().BeSuccessful();
        var responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Contain(nameof(TestIntegrationEvent));
        responseText.Should().Contain(nameof(BlogPostCreatedIntegrationEvent));
    }

    [Fact]
    public async Task Dapr_SubscribeWithoutAnyAssembly_OkAsync()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDaprEventBus(nameof(DaprTests));
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        app.Subscribe();
        await app.StartAsync();
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.GetAsync("/dapr/subscribe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Dapr_MapSubscribeHandler_OkAsync()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDaprClient();
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        app.MapSubscribeHandler();
        await app.StartAsync();
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.GetAsync("/dapr/subscribe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
