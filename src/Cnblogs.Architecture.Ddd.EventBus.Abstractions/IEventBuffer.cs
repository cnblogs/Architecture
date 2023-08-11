namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Buffer for integration events.
/// </summary>
public interface IEventBuffer
{
    /// <summary>
    ///     Number of pending events.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Add an event to buffer.
    /// </summary>
    /// <param name="name">The name of integration event.</param>
    /// <param name="event">The event.</param>
    /// <typeparam name="TEvent">The type of integration event.</typeparam>
    /// <exception cref="EventBufferOverflowException">Throws when the number of events in buffer exceeds <see cref="EventBusOptions.MaximumBufferSize"/>.</exception>
    void Add<TEvent>(string name, TEvent @event)
        where TEvent : IntegrationEvent;

    /// <summary>
    ///     Get an integration event without removing it.
    /// </summary>
    /// <returns>The integration event, <c>null</c> will be returned if buffer is empty.</returns>
    BufferedIntegrationEvent? Peek();

    /// <summary>
    ///     Get an integration event and remove it.
    /// </summary>
    /// <returns>The integration event, <c>null</c> will be returned if buffer is empty.</returns>
    BufferedIntegrationEvent? Pop();
}
