namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     缓存行为定义。
/// </summary>
public enum CacheBehavior
{
    /// <summary>
    ///     不存在时获取新的。
    /// </summary>
    UpdateCacheIfMiss = 1,

    /// <summary>
    ///     不使用缓存。
    /// </summary>
    DisabledCache = 2
}