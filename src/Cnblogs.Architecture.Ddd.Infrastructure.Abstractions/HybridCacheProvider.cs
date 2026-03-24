using Microsoft.Extensions.Caching.Hybrid;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
/// Cache provider using hybrid cache.
/// </summary>
public class HybridCacheProvider(HybridCache hybridCache) : ICacheProvider
{
    private const HybridCacheEntryFlags DisableLocalCache = HybridCacheEntryFlags.DisableLocalCache;
    private const HybridCacheEntryFlags DisableRemoteCache = HybridCacheEntryFlags.DisableDistributedCache;

    /// <inheritdoc />
    public async Task<TResult> GetOrCreateAsync<TResult>(
        string cacheKey,
        Func<CancellationToken, ValueTask<TResult>> factory,
        TimeSpan? remoteExpires = null,
        TimeSpan? localExpires = null,
        string? groupName = null,
        CancellationToken cancellationToken = default)
    {
        if (remoteExpires is null && localExpires is null)
        {
            throw new ArgumentNullException(
                nameof(remoteExpires),
                "Can not disable local and remote cache at the same time");
        }

        var flag = remoteExpires == null
            ? DisableRemoteCache
            : (localExpires == null ? DisableLocalCache : HybridCacheEntryFlags.None);

        var options = new HybridCacheEntryOptions
        {
            Expiration = remoteExpires,
            LocalCacheExpiration = localExpires,
            Flags = flag
        };
        return await hybridCache.GetOrCreateAsync(
            cacheKey,
            factory,
            options,
            groupName == null ? null : [groupName],
            cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        return hybridCache.RemoveAsync(cacheKey.ToLower(), cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        return hybridCache.RemoveByTagAsync(groupName.ToLower(), cancellationToken);
    }
}
