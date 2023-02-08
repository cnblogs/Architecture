using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection.EventBus.Dapr;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Dapr.Client;
using MediatR;
using System.Reflection;

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
    public static IServiceCollection AddDaprEventBus(
        this IServiceCollection services,
        string appName,
        params Assembly[] assemblies)
    {
        services.Configure<DaprOptions>(o => o.AppName = appName);
        services.AddControllers().AddDapr();
        services.AddScoped<IEventBus, DaprEventBus>();

        if (assemblies.Length > 0)
        {
            services.AddMediatR(assemblies);
        }

        return services;
    }

}
