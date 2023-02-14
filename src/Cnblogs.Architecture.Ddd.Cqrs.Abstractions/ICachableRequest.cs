namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Definition for cachable request.
/// </summary>
public interface ICachableRequest
{
    /// <summary>
    ///     Configuration for local cache provider.
    /// </summary>
    CacheBehavior LocalCacheBehavior { get; set; }

    /// <summary>
    ///     Configuration for remote cache provider.
    /// </summary>
    CacheBehavior RemoteCacheBehavior { get; set; }

    /// <summary>
    ///     The expire time for local cache.
    /// </summary>
    TimeSpan? LocalExpires { get; set; }

    /// <summary>
    ///     The expire time for remote cache.
    /// </summary>
    TimeSpan? RemoteExpires { get; set; }

    /// <summary>
    ///     Generate key for cache group, return <c>null</c> for no group.
    /// </summary>
    /// <returns></returns>
    string? CacheGroupKey();

    /// <summary>
    ///     Generate cache key for each request.
    /// </summary>
    /// <returns>The cache key for current request.</returns>
    string CacheKey()
    {
        return string.Join('-', GetCacheKeyParameters().Select(p => p?.ToString()?.ToLower()));
    }

    /// <summary>
    ///     Get parameters for generating cache key, will call <see cref="object.ToString"/> to each object been provided.
    /// </summary>
    /// <returns>The parameter array.</returns>
    object?[] GetCacheKeyParameters();
}