namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     可配置的分布式锁请求。
/// </summary>
public interface IConfigurableLockableRequest : ILockableRequest
{
    /// <summary>
    ///     锁过期时间。
    /// </summary>
    TimeSpan ExpiresIn { get; }
}