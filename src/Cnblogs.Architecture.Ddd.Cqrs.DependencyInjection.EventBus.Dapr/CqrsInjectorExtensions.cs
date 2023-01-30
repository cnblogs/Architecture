using System.Reflection;

using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection.EventBus.Dapr;

/// <summary>
///     添加 Dapr 到 <see cref="CqrsInjector"/>
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     添加 Dapr 实现的 <see cref="IEventBus"/>。
    /// </summary>
    /// <param name="cqrsInjector"><see cref="CqrsInjector"/></param>
    /// <param name="integrationEventAssembly">集成事件所在的 Assembly。</param>
    /// <returns></returns>
    public static CqrsInjector AddDaprEventBus(this CqrsInjector cqrsInjector, Assembly integrationEventAssembly)
    {
        var appName = integrationEventAssembly.GetCustomAttribute<AssemblyAppNameAttribute>();
        if (appName is null)
        {
            throw new InvalidOperationException(
                "No AssemblyAppNameAttribute was found, add attribute to Assembly or specify AppName with AddDaprEventBus(string appName)");
        }

        return cqrsInjector.AddDaprEventBus(appName.Name);
    }

    /// <summary>
    ///     添加 DaprClient
    /// </summary>
    /// <param name="cqrsInjector"><see cref="CqrsInjector"/></param>
    /// <param name="appName">发布事件时使用的 appName。</param>
    /// <returns></returns>
    public static CqrsInjector AddDaprEventBus(this CqrsInjector cqrsInjector, string appName)
    {
        cqrsInjector.Services.Configure<DaprOptions>(o => o.AppName = appName);
        cqrsInjector.Services.AddControllers().AddDapr();
        cqrsInjector.Services.AddScoped<IEventBus, DaprEventBus>();
        return cqrsInjector;
    }
}