using System.Net;
using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using Cnblogs.Architecture.TestIntegrationEvents;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cnblogs.Architecture.IntegrationTests;

public class IntegrationEventPublishTests
{
    [Fact]
    public async Task EventBus_PublishEvent_SuccessAsync()
    {
        // Arrange
        const string data = "hello";
        var builder = new WebApplicationFactory<Program>();
        var eventBusMock = Substitute.For<IEventBusProvider>();
        builder = builder.WithWebHostBuilder(
            b => b.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IEventBusProvider>();
                    services.AddScoped<IEventBusProvider>(_ => eventBusMock);
                }));

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/strings",
            new CreatePayload(false, data));
        var content = await response.Content.ReadAsStringAsync();
        await Task.Delay(1500);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.False(string.IsNullOrEmpty(content));
        await eventBusMock.Received(1).PublishAsync(
            Arg.Any<string>(),
            Arg.Is<TestIntegrationEvent>(t => t.Message == data));
    }

    [Fact]
    public async Task EventBus_Downgrading_DowngradeAsync()
    {
        // Arrange
        const string data = "hello";
        var builder = new WebApplicationFactory<Program>();
        var eventBusMock = Substitute.For<IEventBusProvider>();
        builder = builder.WithWebHostBuilder(
            b => b.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IEventBusProvider>();
                    services.AddScoped<IEventBusProvider>(_ => eventBusMock);
                    services.Configure<EventBusOptions>(
                        o =>
                        {
                            o.FailureCountBeforeDowngrade = 1;
                            o.DowngradeInterval = 3000;
                        });
                }));
        eventBusMock.PublishAsync(Arg.Any<string>(), Arg.Any<IntegrationEvent>())
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/strings",
            new CreatePayload(false, data));
        var content = await response.Content.ReadAsStringAsync();
        await Task.Delay(3000); // hit at 1000ms and 3000ms

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.False(string.IsNullOrEmpty(content));
        await eventBusMock.Received(2).PublishAsync(
            Arg.Any<string>(),
            Arg.Is<TestIntegrationEvent>(t => t.Message == data));
    }

    [Fact]
    public async Task EventBus_MaximumBufferSizeReached_ThrowAsync()
    {
        // Arrange
        const string data = "hello";
        var builder = new WebApplicationFactory<Program>();
        var eventBusMock = Substitute.For<IEventBusProvider>();
        builder = builder.WithWebHostBuilder(
            b => b.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IEventBusProvider>();
                    services.AddScoped<IEventBusProvider>(_ => eventBusMock);
                    services.Configure<EventBusOptions>(
                        o =>
                        {
                            o.MaximumBufferSize = 1;
                            o.FailureCountBeforeDowngrade = 1;
                            o.DowngradeInterval = 3000;
                        });
                }));
        eventBusMock.PublishAsync(Arg.Any<string>(), Arg.Any<IntegrationEvent>())
            .ThrowsAsync(new InvalidOperationException());
        var client = builder.CreateClient();
        await client.PostAsJsonAsync(
            "/api/v1/strings",
            new CreatePayload(false, data));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/strings", new CreatePayload(false, data));

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task EventBus_MaximumBatchSize_OneBatchAsync()
    {
        // Arrange
        const string data = "hello";
        var builder = new WebApplicationFactory<Program>();
        var eventBusMock = Substitute.For<IEventBusProvider>();
        builder = builder.WithWebHostBuilder(
            b => b.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IEventBusProvider>();
                    services.AddScoped<IEventBusProvider>(_ => eventBusMock);
                    services.Configure<EventBusOptions>(
                        o =>
                        {
                            o.MaximumBatchSize = 1;
                            o.FailureCountBeforeDowngrade = 1;
                            o.DowngradeInterval = 3000;
                        });
                }));
        var client = builder.CreateClient();
        for (var i = 0; i < 3; i++)
        {
            // put 3 events
            await client.PostAsJsonAsync("/api/v1/strings", new CreatePayload(false, data));
        }

        // Act
        await Task.Delay(1000);

        // Assert
        await eventBusMock.Received(1).PublishAsync(Arg.Any<string>(), Arg.Any<IntegrationEvent>());
    }

    [Fact]
    public async Task EventBus_DowngradeThenRecover_RecoverAsync()
    {
        // Arrange
        const string data = "hello";
        var builder = new WebApplicationFactory<Program>();
        var eventBusMock = Substitute.For<IEventBusProvider>();
        builder = builder.WithWebHostBuilder(
            b => b.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IEventBusProvider>();
                    services.AddScoped<IEventBusProvider>(_ => eventBusMock);
                    services.Configure<EventBusOptions>(
                        o =>
                        {
                            o.FailureCountBeforeDowngrade = 1;
                            o.DowngradeInterval = 4000;
                        });
                }));
        eventBusMock.PublishAsync(Arg.Any<string>(), Arg.Any<IntegrationEvent>())
            .ThrowsAsync(new InvalidOperationException());
        await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/strings",
            new CreatePayload(false, data));
        await Task.Delay(1000); // failed, now it is downgraded

        // Act
        eventBusMock.PublishAsync(Arg.Any<string>(), Arg.Any<IntegrationEvent>()).Returns(Task.CompletedTask);
        eventBusMock.ClearReceivedCalls();
        await Task.Delay(2000); // recover
        await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/strings",
            new CreatePayload(false, data));
        await Task.Delay(1000);

        // Assert
        await eventBusMock.Received(2)
            .PublishAsync(Arg.Any<string>(), Arg.Is<TestIntegrationEvent>(t => t.Message == data));
    }
}
