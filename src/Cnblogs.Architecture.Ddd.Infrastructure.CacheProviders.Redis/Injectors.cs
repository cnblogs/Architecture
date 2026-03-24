using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
    /// <param name="prefix">The common prefix for redis cache.</param>
    /// <returns></returns>
    public static CqrsInjector AddRedisQueryCache(
        this CqrsInjector injector,
        IConfiguration configuration,
        string sectionName = "Redis",
        string? prefix = null)
    {
        return injector.AddRedisQueryCache(configuration.GetSection(sectionName), prefix);
    }

    /// <summary>
    /// Add redis cache as remote cache.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="section">The configuration section for redis.</param>
    /// <param name="prefix">The common prefix for redis keys.</param>
    /// <returns></returns>
    public static CqrsInjector AddRedisQueryCache(
        this CqrsInjector injector,
        IConfigurationSection section,
        string? prefix = null)
    {
        var option = section.Get<ConfigurationOptions>()
                     ?? throw new InvalidOperationException(
                         $"The given configuration section can not be mapped to {nameof(ConfigurationOptions)}");
        return injector.AddRedisQueryCache(option, prefix);
    }

    /// <summary>
    /// Add redis cache as remote cache.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="prefix">Optional key prefix for redis cache.</param>
    /// <returns></returns>
    public static CqrsInjector AddRedisQueryCache(
        this CqrsInjector injector,
        string connectionString,
        string? prefix = null)
    {
        var options = ConfigurationOptions.Parse(connectionString, true);
        return injector.AddRedisQueryCache(options, prefix);
    }

    private static CqrsInjector AddRedisQueryCache(
        this CqrsInjector injector,
        ConfigurationOptions options,
        string? prefix = null)
    {
        injector.Services.AddStackExchangeRedisCache(o =>
        {
            o.ConfigurationOptions = options;
            o.InstanceName = prefix;
        });
        return injector.AddQueryCache();
    }
}
