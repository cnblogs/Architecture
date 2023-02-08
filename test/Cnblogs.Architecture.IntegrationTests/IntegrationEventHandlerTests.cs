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
    private readonly ITestOutputHelper _testOutputHelper;

    public IntegrationEventHandlerTests(IntegrationTestFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _factory.TestOutputHelper = testOutputHelper;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task IntegrationEventHandler_TestIntegrationEvent_SuccessAsync()
    {
        // Arrange
        var client = _factory.CreateClient();
        var @event = new TestIntegrationEvent(Guid.NewGuid(), DateTimeOffset.Now, "Hello World!");

        // Act
        var subscriptions = await client.GetFromJsonAsync<Subscription[]>("/dapr/subscribe");
        var sub = subscriptions.First(x => x.Route.Contains(nameof(TestIntegrationEvent)));
        var response = await client.PostAsJsonAsync(sub.Route, @event);
        _testOutputHelper.WriteLine("Subscription Route: " + sub.Route);

        // Assert
        response.Should().BeSuccessful();
        response.Headers.Should().ContainKey(Constants.IntegrationEventIdHeaderName)
            .WhoseValue.First().Should().Be(@event.Id.ToString());
    }
}