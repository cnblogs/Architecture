using System.Net;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
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
        builder.Services.AddCqrs(typeof(TestIntegrationEvent).Assembly).AddEventBus(o => o.UseDapr(nameof(DaprTests)));
        builder.WebHost.UseTestServer();

        await using var app = builder.Build();

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
        builder.Services.AddCqrs().AddEventBus(o => o.UseDapr(nameof(DaprTests)));
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
