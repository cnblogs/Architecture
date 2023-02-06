using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Dapr.Client;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.EventBus.Dapr;

/// <summary>
///     Dapr EventBus 实现。
/// </summary>
public class DaprEventBus : IEventBus
{
    private readonly DaprClient _daprClient;
    private readonly DaprOptions _daprOptions;
    private readonly IMediator _mediator;
    private readonly ILogger<DaprEventBus> _logger;

    /// <summary>
    ///     创建一个 DaprEventBus
    /// </summary>
    /// <param name="daprOptions"><see cref="DaprOptions"/></param>
    /// <param name="daprClient"><see cref="DaprClient"/></param>
    /// <param name="logger">日志记录器。</param>
    /// <param name="mediator"><see cref="IMediator"/></param>
    public DaprEventBus(
        IOptions<DaprOptions> daprOptions,
        DaprClient daprClient,
        IMediator mediator,
        ILogger<DaprEventBus> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
        _mediator = mediator;
        _daprOptions = daprOptions.Value;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : IntegrationEvent
    {
        await PublishAsync(typeof(TEvent).Name, @event);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(string eventName, TEvent @event)
        where TEvent : IntegrationEvent
    {
        _logger.LogInformation(
            "Publishing IntegrationEvent, Name: {EventName}, Body: {Event}, TraceId: {TraceId}",
            eventName,
            @event,
            @event.TraceId ?? @event.Id);
        @event.TraceId = TraceId;
        await _daprClient.PublishEventAsync(
            DaprOptions.PubSubName,
            DaprUtils.GetDaprTopicName(_daprOptions.AppName, eventName),
            @event);
    }

    /// <inheritdoc />
    public Task<bool> TryPublishAsync<TEvent>(TEvent @event)
        where TEvent : IntegrationEvent
    {
        return TryPublishAsync(typeof(TEvent).Name, @event);
    }

    /// <inheritdoc />
    public async Task<bool> TryPublishAsync<TEvent>(string eventName, TEvent @event)
        where TEvent : IntegrationEvent
    {
        try
        {
            await PublishAsync(eventName, @event);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Publish IntegrationEvent({Name}) failed, {Event}", eventName, @event);
            return false;
        }
    }

    /// <inheritdoc />
    public Task ReceiveAsync<TEvent>(TEvent receivedEvent)
        where TEvent : IntegrationEvent
    {
        var traceId = receivedEvent.TraceId ?? receivedEvent.Id;
        _logger.LogInformation(
            "Received integration event, Name: {EventName}, Event: {Event}, TraceId: {TraceId}",
            typeof(TEvent).Name,
            receivedEvent,
            traceId);
        TraceId = traceId;
        return _mediator.Publish(receivedEvent);
    }

    /// <inheritdoc />
    public Guid? TraceId { get; set; }
}