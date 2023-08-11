namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     The exception that is thrown when <see cref="IEventBuffer"/> reaches its maximum capacity configured in <see cref="EventBusOptions.MaximumBufferSize"/>.
/// </summary>
public sealed class EventBufferOverflowException : Exception
{
    /// <summary>
    ///     Creates an <see cref="EventBufferOverflowException"/>.
    /// </summary>
    public EventBufferOverflowException()
        : base("Event buffer reached its maximum capacity")
    {
    }
}
