using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Builder for <see cref="EventBusOptions"/>.
/// </summary>
public class EventBusOptionsBuilder
{
    /// <summary>
    ///     Create a <see cref="EventBusOptionsBuilder"/>.
    /// </summary>
    /// <param name="services"></param>
    public EventBusOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    ///     Internal service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    ///     The interval in milliseconds for checking pending integration events.
    /// </summary>
    public int Interval { get; set; } = 1000;

    internal Action<EventBusOptions> GetConfiguration()
    {
        return o =>
        {
            o.Interval = Interval;
        };
    }
}
