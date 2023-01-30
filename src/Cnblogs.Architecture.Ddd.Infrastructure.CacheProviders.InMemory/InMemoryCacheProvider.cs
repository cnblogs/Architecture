using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Microsoft.Extensions.Caching.Memory;

namespace Cnblogs.Architecture.Ddd.Infrastructure.CacheProviders.InMemory;

/// <summary>
/// 使用 <see cref="IMemoryCache"/> 作为基础实现的 <see cref="ILocalCacheProvider"/>
/// </summary>
public sealed class InMemoryCacheProvider : ILocalCacheProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// 构造一个新的 <see cref="InMemoryCacheProvider"/>。
    /// </summary>
    /// <param name="memoryCache">作为实现基础的 <see cref="IMemoryCache"/>。</param>
    /// <param name="dateTimeProvider">时间提供器。</param>
    public InMemoryCacheProvider(IMemoryCache memoryCache, IDateTimeProvider dateTimeProvider)
    {
        _memoryCache = memoryCache;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public Task<bool> AddAsync<TResult>(string cacheKey, TResult value)
    {
        _memoryCache.Set(cacheKey, new CacheEntry<TResult>(value, _dateTimeProvider.UnixSeconds()));
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> AddAsync<TResult>(string cacheKey, TimeSpan expires, TResult value)
    {
        _memoryCache.Set(cacheKey, new CacheEntry<TResult>(value, _dateTimeProvider.UnixSeconds()), expires);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<CacheEntry<TResult>?> GetAsync<TResult>(string cacheKey)
    {
        return Task.FromResult(_memoryCache.Get<CacheEntry<TResult>>(cacheKey));
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
        return Task.FromResult(true);
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
}