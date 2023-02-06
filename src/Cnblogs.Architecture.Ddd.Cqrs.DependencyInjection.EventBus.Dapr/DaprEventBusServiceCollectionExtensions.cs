using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Dapr.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     IServiceCollection extensions for DaprEventBus
/// </summary>
public static class DaprEventBusServiceCollectionExtensions
{
    /// <summary>
    ///      Register <see cref="DaprClient"/> and <see cref="IEventBus"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="appName">The app name used when publishing integration events.</param>
    /// <returns></returns>
    public static IServiceCollection AddDaprEventBus(this IServiceCollection services, string appName)
    {
        services.Configure<DaprOptions>(o => o.AppName = appName);
        services.AddControllers().AddDapr();
        services.AddScoped<IEventBus, DaprEventBus>();
        return services;
    }
}
