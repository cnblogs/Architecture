using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Infrastructure.CacheProviders.InMemory;

/// <summary>
/// <see cref="CqrsInjector"/> 的扩展方法。
/// </summary>
public static class Injectors
{
    /// <summary>
    /// 添加 <see cref="InMemoryCacheProvider"/> 作为本地缓存。
    /// </summary>
    /// <param name="injector"><see cref="CqrsInjector"/>。</param>
    /// <param name="configure">额外缓存配置。</param>
    /// <returns></returns>
    public static CqrsInjector AddInMemoryCache(
        this CqrsInjector injector,
        Action<CacheableRequestOptions>? configure = null)
    {
        return injector.AddLocalQueryCache<InMemoryCacheProvider>(configure);
    }
}