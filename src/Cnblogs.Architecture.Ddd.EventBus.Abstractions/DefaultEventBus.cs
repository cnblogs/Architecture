using MediatR;
using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Default implementation for <see cref="IEventBus"/>
/// </summary>
public class DefaultEventBus : IEventBus
{
    private readonly IEventBuffer _eventBuffer;
    private readonly IMediator _mediator;
    private readonly ILogger<DefaultEventBus> _logger;

    /// <summary>
    ///     Create a <see cref="DefaultEventBus"/> instance.
    /// </summary>
    /// <param name="eventBuffer">The underlying event buffer.</param>
    /// <param name="mediator">The IMediator.</param>
    /// <param name="logger">The logger.</param>
    public DefaultEventBus(IEventBuffer eventBuffer, IMediator mediator, ILogger<DefaultEventBus> logger)
    {
        _eventBuffer = eventBuffer;
        _logger = logger;
        _mediator = mediator;
    }

    /// <inheritdoc />
    public Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : IntegrationEvent
    {
        return PublishAsync(typeof(TEvent).Name, @event);
    }

    /// <inheritdoc />
    public Task PublishAsync<TEvent>(string eventName, TEvent @event)
        where TEvent : IntegrationEvent
    {
        @event.TraceId = TraceId;
        _eventBuffer.Add(eventName, @event);
        return Task.CompletedTask;
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
