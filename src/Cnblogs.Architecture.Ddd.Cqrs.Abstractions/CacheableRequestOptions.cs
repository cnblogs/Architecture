namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// 缓存配置。
/// </summary>
public class CacheableRequestOptions
{
    /// <summary>
    /// 如果获取失败抛出异常。
    /// </summary>
    public bool ThrowIfFailedOnGet { get; set; }

    /// <summary>
    /// 如果更新失败则抛出异常。
    /// </summary>
    public bool ThrowIfFailedOnUpdate { get; set; }

    /// <summary>
    /// 如果清除缓存失败则抛出异常，可能被 <see cref="InvalidCacheRequest"/> 中的 <see cref="InvalidCacheRequest.ThrowIfFailed"/> 覆盖。
    /// </summary>
    public bool ThrowIfFailedOnRemove { get; set; }
}