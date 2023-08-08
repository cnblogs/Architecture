namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     The integration event stored in buffer.
/// </summary>
/// <param name="Name">The event name.</param>
/// <param name="Event">The event data.</param>
public record BufferedIntegrationEvent(string Name, IntegrationEvent Event);
