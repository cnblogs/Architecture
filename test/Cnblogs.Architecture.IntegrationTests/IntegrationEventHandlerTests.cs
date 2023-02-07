using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Http.Json;
using Cnblogs.Architecture.IntegrationTestProject;
using Cnblogs.Architecture.IntegrationTestProject.EventHandlers;
using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class IntegrationEventHandlerTests
{
    private readonly IntegrationTestFactory _factory;

    public IntegrationEventHandlerTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task IntegrationEventHandler_TestIntegrationEvent_SuccessAsync()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var subscriptions = await client.GetFromJsonAsync<Subscription[]>("/dapr/subscribe");

        // Assert
        subscriptions.Should().NotBeNullOrEmpty();

        // Act
        var sub = subscriptions.FirstOrDefault(s => s.Route.Contains(nameof(TestIntegrationEvent)));

        // Assert
        sub.Should().NotBeNull();

        Debug.WriteLine("Subscription Route: " + sub.Route);

        // Act
        var @event = new TestIntegrationEvent(Guid.NewGuid(), DateTimeOffset.Now, "Hello World!");
        var response = await client.PostAsJsonAsync(sub.Route, @event);

        // Assert
        response.Should().BeSuccessful();
        Assert.True(response.Headers.TryGetValues(Constants.IntegrationEventIdHeaderName, out var values));
        values.First().Should().Be(@event.Id.ToString());
    }
}