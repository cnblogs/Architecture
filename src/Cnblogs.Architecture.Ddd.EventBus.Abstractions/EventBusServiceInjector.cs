using System.Reflection;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Extension methods for injecting <see cref="IEventBus"/> to service collection.
/// </summary>
public static class EventBusServiceInjector
{
    /// <summary>
    ///     Add event bus for integration event support.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">Extra configurations for event bus.</param>
    /// <param name="handlerAssemblies">The assemblies for handlers.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        Action<EventBusOptionsBuilder>? configuration = null,
        params Assembly[] handlerAssemblies)
    {
        services.TryAddSingleton<IEventBuffer, InMemoryEventBuffer>();
        services.TryAddScoped<IEventBus, DefaultEventBus>();
        services.AddHostedService<PublishIntegrationEventHostedService>();
        var builder = new EventBusOptionsBuilder(services);
        configuration?.Invoke(builder);
        services.Configure(builder.GetConfiguration());
        if (handlerAssemblies.Length > 0)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(handlerAssemblies));
        }

        return services;
    }

    /// <summary>
    ///     Add event bus for integration event support.
    /// </summary>
    /// <param name="cqrsInjector">The <see cref="CqrsInjector"/>.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="handlerAssemblies">The assemblies for handlers.</param>
    /// <returns></returns>
    public static CqrsInjector AddEventBus(
        this CqrsInjector cqrsInjector,
        Action<EventBusOptionsBuilder>? configuration = null,
        params Assembly[] handlerAssemblies)
    {
        cqrsInjector.Services.AddEventBus(configuration, handlerAssemblies);
        return cqrsInjector;
    }
}
