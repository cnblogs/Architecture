﻿using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Cnblogs.Architecture.IntegrationTestProject.EventHandlers;
using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit.Abstractions;
using static Cnblogs.Architecture.IntegrationTestProject.Constants;

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
        builder.Logging.AddSerilog(logger => logger.WriteTo.InMemory().WriteTo.Console());
        builder.Services.AddEventBus(
            o => o.UseDapr(nameof(IntegrationEventHandlerTests)),
            typeof(TestIntegrationEventHandler).Assembly);
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.Subscribe<TestIntegrationEvent>();
        await app.StartAsync();
        var client = app.GetTestClient();
        var @event = new TestIntegrationEvent(Guid.NewGuid(), DateTimeOffset.Now, $"Hello World! {Guid.NewGuid()}");

        // Act
        var subscriptions = await client.GetFromJsonAsync<Subscription[]>("/dapr/subscribe");
        var sub = subscriptions!.First(x => x.Route.Contains(nameof(TestIntegrationEvent)));
        var response = await client.PostAsJsonAsync(sub.Route, @event);
        _testOutputHelper.WriteLine("Subscription Route: " + sub.Route);

        // Assert
        response.Should().BeSuccessful();
        InMemorySink.Instance
            .Should().HaveMessage(LogTemplates.HandledIntegratonEvent).Appearing().Once()
            .WithProperty("event").HavingADestructuredObject().WithProperty("Id").WithValue(@event.Id);
    }
}
