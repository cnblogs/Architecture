using System.Reflection;
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
    ///     Scan assemblies for <see cref="IEnricher{T}" /> implementations and register them as scoped services.
    ///     When an enricher implements <c>IEnricher&lt;T&gt;</c> where <c>T</c> is an interface or abstract class,
    ///     it will automatically be registered for all concrete types implementing that interface/abstract class.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <param name="maxInterfaceImplementations">
    ///     The maximum number of concrete implementations allowed when expanding an interface-based enricher.
    ///     Defaults to 1000. If exceeded, an <see cref="InvalidOperationException" /> is thrown.
    /// </param>
    /// <returns></returns>
    public CqrsInjector AddEnrichers(Assembly[] assemblies, int maxInterfaceImplementations = 1000)
    {
        Services.TryAddSingleton<EnricherMappingCache>();
        Services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(EnricherBehavior<,>));

        var concreteTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToList();

        var enricherTypes = concreteTypes
            .SelectMany(t => t.GetInterfaces(), (t, i) => (Implementation: t, Service: i))
            .Where(x => x.Service.IsGenericType && x.Service.GetGenericTypeDefinition() == typeof(IEnricher<>));

        foreach (var (impl, service) in enricherTypes)
        {
            RegisterEnricher(impl, service, concreteTypes, maxInterfaceImplementations);
        }

        return this;
    }

    private void RegisterEnricher(
        Type impl, Type service, List<Type> concreteTypes, int maxInterfaceImplementations)
    {
        var targetType = service.GetGenericArguments()[0];
        if (targetType is not { IsInterface: true } and not { IsAbstract: true })
        {
            Services.AddScoped(service, impl);
            return;
        }

        var implementations = concreteTypes.Where(targetType.IsAssignableFrom).ToList();
        if (implementations.Count > maxInterfaceImplementations)
        {
            throw new InvalidOperationException(
                $"IEnricher<{targetType.Name}> matches {implementations.Count} concrete types, "
                + $"which exceeds the limit of {maxInterfaceImplementations}. "
                + "Consider narrowing the interface or increasing the limit.");
        }

        Services.AddScoped(service, impl);

        foreach (var concreteTarget in implementations)
        {
            var closedService = typeof(IEnricher<>).MakeGenericType(concreteTarget);
            var adapterType = typeof(InterfaceEnricherAdapter<,>).MakeGenericType(targetType, concreteTarget);
            Services.AddScoped(closedService, adapterType);
        }
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
    ///     添加默认 ID 提供器，使用随机 instanceId。
    /// </summary>
    /// <returns></returns>
    public CqrsInjector AddDefaultIdProvider()
    {
        return AddDefaultIdProvider(Random.Shared.Next(1000));
    }

    /// <summary>
    ///     添加默认 ID 提供器。
    /// </summary>
    /// <param name="instanceId">实例Id。也可以用 PodId，在应用层面唯一。</param>
    /// <param name="configures">ID 的额外配置。</param>
    /// <returns></returns>
    public CqrsInjector AddDefaultIdProvider(int instanceId, Action<DefaultIdProviderOption>? configures = null)
    {
        var option = new DefaultIdProviderOption { InstanceId = instanceId, };
        configures?.Invoke(option);
        Services.AddSingleton<IIdProvider>(sp => new DefaultIdProvider(
            sp.GetRequiredService<IDateTimeProvider>(),
            option));
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
