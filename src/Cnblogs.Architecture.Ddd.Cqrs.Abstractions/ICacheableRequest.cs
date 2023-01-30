namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义可缓存的请求
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    ///     本地缓存配置。
    /// </summary>
    CacheBehavior LocalCacheBehavior { get; set; }

    /// <summary>
    ///     远程缓存配置。
    /// </summary>
    CacheBehavior RemoteCacheBehavior { get; set; }

    /// <summary>
    ///     本地缓存过期时间。
    /// </summary>
    TimeSpan? LocalExpires { get; set; }

    /// <summary>
    ///     远程缓存过期时间。
    /// </summary>
    TimeSpan? RemoteExpires { get; set; }

    /// <summary>
    ///     获取缓存分组键，<c>null</c> 代表不分组。
    /// </summary>
    /// <returns></returns>
    string? CacheGroupKey();

    /// <summary>
    ///     获取缓存键。
    /// </summary>
    /// <returns></returns>
    string CacheKey()
    {
        return string.Join('-', GetCacheKeyParameters().Select(p => p?.ToString()?.ToLower()));
    }

    /// <summary>
    ///     获取组成缓存键的参数。
    /// </summary>
    /// <returns></returns>
    object?[] GetCacheKeyParameters();
}