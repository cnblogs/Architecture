using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Options for event bus.
/// </summary>
public class EventBusOptions
{
    /// <summary>
    ///     The service collection for
    /// </summary>
    public IServiceCollection? Services { get; set; }

    /// <summary>
    ///     Interval for publish integration event.
    /// </summary>
    public int Interval { get; set; } = 1;
}
