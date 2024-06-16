using StackExchange.Redis;

namespace Cnblogs.Architecture.Ddd.Infrastructure.CacheProviders.Redis;

/// <summary>
/// Options for redis connection.
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Prefix for all redis keys.
    /// </summary>
    public string Prefix { get; set; } = "cache_";

    /// <summary>
    /// The number of database to use.
    /// </summary>
    public int Database { get; set; } = -1;

    /// <summary>
    /// The redis configuration, https://stackexchange.github.io/StackExchange.Redis/Configuration
    /// </summary>
    public ConfigurationOptions Configure { get; set; } = new();
}
