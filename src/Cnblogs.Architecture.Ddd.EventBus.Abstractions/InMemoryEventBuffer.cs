using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Implementation of <see cref="IEventBuffer"/> using <see cref="ConcurrentQueue{T}"/>.
/// </summary>
public class InMemoryEventBuffer : IEventBuffer
{
    private readonly ConcurrentQueue<BufferedIntegrationEvent> _queue = new();
    private readonly EventBusOptions _options;

    /// <summary>
    ///     Creates an <see cref="InMemoryEventBuffer"/>.
    /// </summary>
    /// <param name="options">The Eventbus options.</param>
    public InMemoryEventBuffer(IOptions<EventBusOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public int Count => _queue.Count;

    /// <inheritdoc />
    public void Add<TEvent>(string name, TEvent @event)
        where TEvent : IntegrationEvent
    {
        if (_queue.Count >= _options.MaximumBufferSize)
        {
            throw new EventBufferOverflowException();
        }

        _queue.Enqueue(new BufferedIntegrationEvent(name, @event));
    }

    /// <inheritdoc />
    public BufferedIntegrationEvent? Peek()
    {
        return _queue.TryPeek(out var @event) ? @event : null;
    }

    /// <inheritdoc />
    public BufferedIntegrationEvent? Pop()
    {
        return _queue.TryDequeue(out var @event) ? @event : null;
    }
}
