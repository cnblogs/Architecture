using System.Reflection;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;

namespace Cnblogs.Architecture.Ddd.EventBus.Dapr;

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
    [Obsolete("Use builder.AddCqrs().AddEventBus(o => o.UseDapr(assembly)) instead.", true)]
    public static CqrsInjector AddDaprEventBus(this CqrsInjector cqrsInjector, Assembly integrationEventAssembly)
    {
        return cqrsInjector.AddEventBus(o => o.UseDapr(integrationEventAssembly));
    }

    /// <summary>
    ///     添加 DaprClient
    /// </summary>
    /// <param name="cqrsInjector"><see cref="CqrsInjector"/></param>
    /// <param name="appName">发布事件时使用的 appName。</param>
    /// <returns></returns>
    [Obsolete("Use builder.AddCqrs().AddEventBus(o => o.UseDapr(appName)) instead.", true)]
    public static CqrsInjector AddDaprEventBus(this CqrsInjector cqrsInjector, string appName)
    {
        cqrsInjector.AddEventBus(o => o.UseDapr(appName));
        return cqrsInjector;
    }
}
