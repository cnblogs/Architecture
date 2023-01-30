namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     使用分布式锁后的响应。
/// </summary>
public interface ILockableResponse
{
    /// <summary>
    /// 是否出现并发错误（获取不到锁）
    /// </summary>
    bool IsConcurrentError { get; init; }

    /// <summary>
    ///     是否成功获取到锁。
    /// </summary>
    bool LockAcquired { get; set; }
}