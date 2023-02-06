using System.Diagnostics;
using System.Net;
using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.IntegrationTests;

public class DaprTests
{
    public DaprTests()
    {
    }

    [Fact]
    public async Task Dapr_SubscribeEndpoint_OkAsync()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDaprEventBus(nameof(DaprTests));
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        app.Subscribe<TestIntegrationEvent>();
        await app.StartAsync();
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.GetAsync("/dapr/subscribe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Contain(nameof(TestIntegrationEvent));
    }

    [Fact]
    public async Task Dapr_Subscribe_Without_Any_Assembly_OkAsync()
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
