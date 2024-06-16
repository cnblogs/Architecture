using System.Text.Json;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Cnblogs.Architecture.Ddd.Infrastructure.CacheProviders.Redis;

/// <summary>
/// Remote cache provider implemented with Redis.
/// </summary>
public class RedisCacheProvider
    : IRemoteCacheProvider
{
    private readonly RedisOptions _options;
    private readonly IDatabaseAsync _database;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Remote cache provider implemented with Redis.
    /// </summary>
    /// <param name="multiplexer">The underlying multiplexer.</param>
    /// <param name="options">The options for this provider.</param>
    /// <param name="dateTimeProvider">The datetime provider.</param>
    public RedisCacheProvider(
        ConnectionMultiplexer multiplexer,
        IOptions<RedisOptions> options,
        IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _database = multiplexer.GetDatabase(_options.Database);
    }

    /// <inheritdoc />
    public Task<bool> AddAsync<TResult>(string cacheKey, TResult value)
    {
        return _database.StringSetAsync(GetCacheKey(cacheKey), Serialize(value));
    }

    /// <inheritdoc />
    public Task<bool> AddAsync<TResult>(string cacheKey, TimeSpan expires, TResult value)
    {
        return _database.StringSetAsync(GetCacheKey(cacheKey), Serialize(value), expires);
    }

    /// <inheritdoc />
    public async Task<CacheEntry<TResult>?> GetAsync<TResult>(string cacheKey)
    {
        var json = await _database.StringGetAsync(GetCacheKey(cacheKey));
        if (json.IsNullOrEmpty)
        {
            return null;
        }

        return DeSerialize<TResult>(json!);
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string cacheKey)
    {
        return _database.KeyDeleteAsync(GetCacheKey(cacheKey));
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync<TResult>(string cacheKey, TResult value)
    {
        return AddAsync(cacheKey, value);
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync<TResult>(string cacheKey, TResult value, TimeSpan expires)
    {
        return AddAsync(cacheKey, expires, value);
    }

    private string GetCacheKey(string key) => $"{_options.Prefix}{key}";

    private string Serialize<TResult>(TResult result)
        => JsonSerializer.Serialize(new CacheEntry<TResult>(result, _dateTimeProvider.UnixSeconds()));

    private static CacheEntry<TResult>? DeSerialize<TResult>(string json)
        => JsonSerializer.Deserialize<CacheEntry<TResult>>(json);
}
