using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

/// <summary>
///     Cqrs 配置器。
/// </summary>
public class CqrsInjector
{
    /// <summary>
    ///     创建一个 <see cref="CqrsInjector" /> 的新实例。
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    internal CqrsInjector(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    ///     内部的 <see cref="IServiceCollection" />。
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    ///     添加自定义时间提供器。
    /// </summary>
    /// <typeparam name="TDateTimeProvider">时间提供器。</typeparam>
    /// <returns></returns>
    public CqrsInjector AddDateTimeProvider<TDateTimeProvider>()
        where TDateTimeProvider : class, IDateTimeProvider
    {
        Services.AddSingleton<IDateTimeProvider, TDateTimeProvider>();
        return this;
    }

    /// <summary>
    ///     使用默认时间和随机数提供器，可以在依赖注入中使用 <see cref="IDateTimeProvider" /> 和 <see cref="IRandomProvider" /> 获得对应实例。
    /// </summary>
    /// <returns></returns>
    public CqrsInjector AddDefaultDateTimeAndRandomProvider()
    {
        return AddRandomProvider<DefaultRandomProvider>().AddDateTimeProvider<DefaultDateTimeProvider>();
    }

    /// <summary>
    ///     启用分布式锁。
    /// </summary>
    /// <typeparam name="TDistributedLockProvider">分布式锁提供器。</typeparam>
    /// <returns></returns>
    public CqrsInjector AddDistributionLock<TDistributedLockProvider>()
        where TDistributedLockProvider : class, IDistributedLockProvider
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LockableRequestBehavior<,>));
        Services.AddScoped<IDistributedLockProvider, TDistributedLockProvider>();
        return this;
    }

    /// <summary>
    ///     Add cache to the query pipeline, auto cache queries implemented <see cref="ICachableRequest" />.
    /// </summary>
    /// <param name="setupAction"><see cref="HybridCacheOptions"/></param>
    /// <param name="builderAction">Hybrid cache build that allows you to configure serializer, etc.</param>
    /// <returns></returns>
    public CqrsInjector AddHybridQueryCache(
        Action<HybridCacheOptions>? setupAction = null,
        Action<IHybridCacheBuilder>? builderAction = null)
    {
        AddCacheBehaviorPipeline();
        setupAction ??= h => h.ReportTagMetrics = false;
        var builder = Services.AddHybridCache(setupAction);
        builderAction?.Invoke(builder);
        Services.AddScoped<ICacheProvider, HybridCacheProvider>();
        return this;
    }

    /// <summary>
    ///     Use default implementation of <see cref="IFileProvider"/> that accesses file system directly.
    /// </summary>
    /// <returns></returns>
    public CqrsInjector AddDefaultFileProvider()
    {
        return AddFileProvider<DefaultFileProvider>();
    }

    /// <summary>
    ///     Use given implementation of <see cref="IFileProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">The implementation type.</typeparam>
    /// <returns></returns>
    public CqrsInjector AddFileProvider<TProvider>()
        where TProvider : class, IFileProvider
    {
        Services.AddScoped<IFileProvider, TProvider>();
        return this;
    }

    /// <summary>
    ///     Use given implementation of <see cref="IFileDeliveryProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">The type of implementation.</typeparam>
    /// <returns></returns>
    public CqrsInjector AddFileDeliveryProvider<TProvider>()
        where TProvider : class, IFileDeliveryProvider
    {
        Services.AddScoped<IFileDeliveryProvider, TProvider>();
        return this;
    }

    /// <summary>
    ///     添加自定义随机数提供器。
    /// </summary>
    /// <typeparam name="TRandomProvider">随机数提供器。</typeparam>
    /// <returns></returns>
    public CqrsInjector AddRandomProvider<TRandomProvider>()
        where TRandomProvider : class, IRandomProvider
    {
        Services.AddSingleton<IRandomProvider, TRandomProvider>();
        return this;
    }

    /// <summary>
    ///     添加默认 ID 提供器。
    /// </summary>
    /// <param name="machineId">机器Id。也可以用 PodId，在应用层面唯一。</param>
    /// <returns></returns>
    public CqrsInjector AddDefaultIdProvider(int machineId)
    {
        Services.AddSingleton<IIdProvider>(sp => new DefaultIdProvider(
            sp.GetRequiredService<IDateTimeProvider>(),
            machineId));
        return this;
    }

    /// <summary>
    ///     添加自定义 ID 提供器。
    /// </summary>
    /// <typeparam name="TIdProvider">ID 提供器。</typeparam>
    /// <returns></returns>
    public CqrsInjector AddIdProvider<TIdProvider>()
        where TIdProvider : class, IIdProvider
    {
        Services.AddSingleton<IIdProvider, TIdProvider>();
        return this;
    }

    private void AddCacheBehaviorPipeline()
    {
        Services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(CacheableRequestBehavior<,>));
    }
}
