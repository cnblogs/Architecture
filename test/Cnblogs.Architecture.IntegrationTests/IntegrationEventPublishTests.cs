using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Cnblogs.Architecture.IntegrationTests;

public class IntegrationEventPublishTests
{
    [Fact]
    public async Task EventBus_PublishEvent_SuccessAsync()
    {
        // Arrange
        const string data = "hello";
        var builder = new WebApplicationFactory<Program>();
        var eventBusMock = new Mock<IEventBusProvider>();
        builder = builder.WithWebHostBuilder(
            b => b.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IEventBusProvider>();
                    services.AddScoped<IEventBusProvider>(_ => eventBusMock.Object);
                }));

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/strings",
            new CreatePayload(false, data));
        var content = await response.Content.ReadAsStringAsync();
        await Task.Delay(1500);

        // Assert
        response.Should().BeSuccessful();
        content.Should().BeNullOrEmpty();
        eventBusMock.Verify(
            x => x.PublishAsync(It.IsAny<string>(), It.Is<TestIntegrationEvent>(t => t.Message == data)),
            Times.Once);
    }
}
