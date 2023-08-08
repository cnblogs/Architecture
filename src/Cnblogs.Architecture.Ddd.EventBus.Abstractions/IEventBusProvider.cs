namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Provider contract for event bus.
/// </summary>
public interface IEventBusProvider
{
    /// <summary>
    ///     Emit an integration event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="event">The event body.</param>
    /// <returns></returns>
    Task PublishAsync(string eventName, IntegrationEvent @event);
}
