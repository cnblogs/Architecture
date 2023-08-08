using System.Collections.Concurrent;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Implementation of <see cref="IEventBuffer"/> using <see cref="ConcurrentQueue{T}"/>.
/// </summary>
public class InMemoryEventBuffer : IEventBuffer
{
    private readonly ConcurrentQueue<BufferedIntegrationEvent> _queue = new();

    /// <inheritdoc />
    public int Count => _queue.Count;

    /// <inheritdoc />
    public void Add<TEvent>(string name, TEvent @event)
        where TEvent : IntegrationEvent
    {
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
