using System.Reflection;
using System.Text.Json;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.EventBus.Dapr;

/// <summary>
///     Injector methods for dapr event bus.
/// </summary>
public static class DaprEventBusInjector
{
    /// <summary>
    ///     Use dapr as event bus provider.
    /// </summary>
    /// <param name="builder">The <see cref="EventBusOptionsBuilder"/>.</param>
    /// <param name="integrationEventAssembly">The assembly of integration events of current app.</param>
    /// <returns></returns>
    public static EventBusOptionsBuilder UseDapr(this EventBusOptionsBuilder builder, Assembly integrationEventAssembly)
    {
        var appName = integrationEventAssembly.GetCustomAttribute<AssemblyAppNameAttribute>();
        if (appName is null)
        {
            throw new InvalidOperationException(
                "No AssemblyAppNameAttribute was found, add attribute to Assembly or specify AppName with AddDaprEventBus(string appName)");
        }

        return builder.UseDapr(appName.Name);
    }

    /// <summary>
    ///     Use dapr as event bus provider.
    /// </summary>
    /// <param name="builder">The <see cref="EventBusOptionsBuilder"/>.</param>
    /// <param name="appName">The name of current app.</param>
    /// <returns></returns>
    public static EventBusOptionsBuilder UseDapr(this EventBusOptionsBuilder builder, string appName)
    {
        var services = builder.Services;
        services.Configure<DaprOptions>(o => o.AppName = appName);
        services.AddControllers().AddDapr();
        services.AddScoped<IEventBusProvider, DaprEventBusProvider>();
        return builder;
    }
}
