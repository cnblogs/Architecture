using System.Net.Http.Json;
using Cnblogs.Architecture.IntegrationTestProject;
using Cnblogs.Architecture.IntegrationTestProject.EventHandlers;
using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Cnblogs.Architecture.IntegrationTests;

public class IntegrationEventHandlerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IntegrationEventHandlerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task IntegrationEventHandler_TestIntegrationEvent_SuccessAsync()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services
            .AddDaprEventBus(nameof(IntegrationEventHandlerTests), typeof(TestIntegrationEventHandler).Assembly)
            .AddHttpContextAccessor();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.Subscribe<TestIntegrationEvent>();
        await app.StartAsync();
        var client = app.GetTestClient();
        var @event = new TestIntegrationEvent(Guid.NewGuid(), DateTimeOffset.Now, "Hello World!");

        // Act
        var subscriptions = await client.GetFromJsonAsync<Subscription[]>("/dapr/subscribe");
        var sub = subscriptions!.First(x => x.Route.Contains(nameof(TestIntegrationEvent)));
        var response = await client.PostAsJsonAsync(sub.Route, @event);
        _testOutputHelper.WriteLine("Subscription Route: " + sub.Route);

        // Assert
        response.Should().BeSuccessful();
        response.Headers.Should().ContainKey(Constants.IntegrationEventIdHeaderName)
            .WhoseValue.First().Should().Be(@event.Id.ToString());
    }
}