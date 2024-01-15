using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.EventBus.Dapr;

/// <summary>
///     Implementations for <see cref="IEventBusProvider"/> using Dapr.
/// </summary>
public class DaprEventBusProvider : IEventBusProvider
{
    private readonly DaprClient _daprClient;
    private readonly DaprOptions _daprOptions;
    private readonly ILogger<DaprEventBusProvider> _logger;

    /// <summary>
    ///     Create a <see cref="DaprEventBusProvider"/>.
    /// </summary>
    /// <param name="daprClient">The underlying dapr client.</param>
    /// <param name="daprOptions">The options for dapr.</param>
    /// <param name="logger">The logger.</param>
    public DaprEventBusProvider(
        DaprClient daprClient,
        IOptions<DaprOptions> daprOptions,
        ILogger<DaprEventBusProvider> logger)
    {
        _daprClient = daprClient;
        _daprOptions = daprOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(string eventName, IntegrationEvent @event)
    {
        _logger.LogInformation(
            "Publishing IntegrationEvent, PubSub: {PubSubName}, TopicName: {TopicName}, Name: {EventName}, Body: {Event}, TraceId: {TraceId}",
            DaprOptions.PubSubName,
            DaprUtils.GetDaprTopicName(_daprOptions.AppName, eventName),
            eventName,
            @event,
            @event.TraceId ?? @event.Id);
        object data = @event; // do not provide type information to serializer since it's base class.
        await _daprClient.PublishEventAsync(
            DaprOptions.PubSubName,
            DaprUtils.GetDaprTopicName(_daprOptions.AppName, eventName),
            data);
    }
}
