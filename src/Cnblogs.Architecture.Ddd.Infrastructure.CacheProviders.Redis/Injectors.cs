using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Cnblogs.Architecture.Ddd.Infrastructure.CacheProviders.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Injectors for redis cache provider.
/// </summary>
public static class Injectors
{
    /// <summary>
    /// Add redis cache as remote cache.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <param name="sectionName">The configuration section name for redis, defaults to Redis.</param>
    /// <param name="configure">The optional configuration.</param>
    /// <returns></returns>
    public static CqrsInjector AddRedisCache(
        this CqrsInjector injector,
        IConfiguration configuration,
        string sectionName = "Redis",
        Action<CacheableRequestOptions>? configure = null)
    {
        return AddRedisCache(injector, configuration.GetSection(sectionName), configure);
    }

    /// <summary>
    /// Add redis cache as remote cache.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="section">The configuration section for redis.</param>
    /// <param name="configure">The optional configuration.</param>
    /// <returns></returns>
    public static CqrsInjector AddRedisCache(
        this CqrsInjector injector,
        IConfigurationSection section,
        Action<CacheableRequestOptions>? configure = null)
    {
        injector.Services.Configure<RedisOptions>(section);
        return AddRedisCache(injector, configure);
    }

    /// <summary>
    /// Add redis cache as remote cache.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="redisConfigure">Optional configuration for redis options.</param>
    /// <param name="configure">The configure for cacheable request options.</param>
    /// <returns></returns>
    public static CqrsInjector AddRedisCache(
        this CqrsInjector injector,
        string connectionString,
        Action<RedisOptions>? redisConfigure = null,
        Action<CacheableRequestOptions>? configure = null)
    {
        var options = ConfigurationOptions.Parse(connectionString, true);
        injector.Services.Configure<RedisOptions>(o =>
        {
            o.Configure = options;
            redisConfigure?.Invoke(o);
        });
        return AddRedisCache(injector, configure);
    }

    private static CqrsInjector AddRedisCache(
        this CqrsInjector injector,
        Action<CacheableRequestOptions>? configure = null)
    {
        injector.Services.AddSingleton(
            sp => ConnectionMultiplexer.Connect(sp.GetRequiredService<IOptions<RedisOptions>>().Value.Configure));
        return injector.AddRemoteQueryCache<RedisCacheProvider>(configure);
    }
}
