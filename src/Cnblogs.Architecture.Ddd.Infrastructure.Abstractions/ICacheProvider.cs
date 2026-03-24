namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     缓存提供器。
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Get or create cache entry.
    /// </summary>
    /// <param name="cacheKey">cache key.</param>
    /// <param name="factory">factory used when cache miss.</param>
    /// <param name="remoteExpires">remote expiration</param>
    /// <param name="localExpires">how long to cache</param>
    /// <param name="groupName">group key for cache.</param>
    /// <param name="cancellationToken">cancellation token to use.</param>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns></returns>
    Task<TResult> GetOrCreateAsync<TResult>(
        string cacheKey,
        Func<CancellationToken, ValueTask<TResult>> factory,
        TimeSpan? remoteExpires = null,
        TimeSpan? localExpires = null,
        string? groupName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove cache associated with <paramref name="cacheKey"/>
    /// </summary>
    /// <param name="cacheKey">The key of cache.</param>
    /// <param name="cancellationToken"></param>
    ValueTask RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove all the cache in the group.
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask RemoveGroupAsync(string groupName, CancellationToken cancellationToken = default);
}
