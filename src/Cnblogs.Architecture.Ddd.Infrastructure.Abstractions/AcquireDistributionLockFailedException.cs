namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     获取分布式锁失败异常。
/// </summary>
public class AcquireDistributionLockFailedException : Exception
{
    /// <summary>
    ///     抛出分布式锁失败异常。
    /// </summary>
    /// <param name="lockKey">请求的key。</param>
    public AcquireDistributionLockFailedException(string lockKey)
    {
        LockKey = lockKey;
    }

    /// <summary>
    ///     抛出分布式锁失败异常。
    /// </summary>
    /// <param name="message">异常信息。</param>
    /// <param name="lockKey">请求的key。</param>
    public AcquireDistributionLockFailedException(string? message, string lockKey)
        : base(message)
    {
        LockKey = lockKey;
    }

    /// <summary>
    ///     请求锁对应的 key。
    /// </summary>
    public string LockKey { get; }
}