using System.Reflection;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Dapr.Client;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     IServiceCollection extensions for DaprEventBus
/// </summary>
public static class DaprEventBusServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="DaprClient"/> and <see cref="IEventBus"/>.
    /// The alternative is using services.AddCqrs().AddDaprEventBus() in <see cref="CqrsInjectorExtensions"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="appName">The app name used when publishing integration events.</param>
    /// <param name="assemblies">Assemblies to scan by MediatR</param>
    /// <returns></returns>
    [Obsolete("use services.AddEventBus(o => o.UseDapr(), assemblies) instead.", true)]
    public static IServiceCollection AddDaprEventBus(
        this IServiceCollection services,
        string appName,
        params Assembly[] assemblies)
    {
        services.AddEventBus(o => o.UseDapr(appName), assemblies);
        return services;
    }
}
