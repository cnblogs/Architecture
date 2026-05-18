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
        string[]? groupNames = null,
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

        if (groupNames?.Length > 0)
        {
            groupNames = groupNames.Select(x => x.ToLowerInvariant()).ToArray();
        }

        return await hybridCache.GetOrCreateAsync(
            cacheKey,
            factory,
            options,
            groupNames,
            cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        return hybridCache.RemoveAsync(cacheKey.ToLowerInvariant(), cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveGroupAsync(string[] groupNames, CancellationToken cancellationToken = default)
    {
        var lowered = groupNames.Select(x => x.ToLowerInvariant()).ToArray();
        return hybridCache.RemoveByTagAsync(lowered, cancellationToken);
    }
}
