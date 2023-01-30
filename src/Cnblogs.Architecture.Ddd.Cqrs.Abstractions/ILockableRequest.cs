namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义需要分布式锁的请求
/// </summary>
public interface ILockableRequest
{
    /// <summary>
    ///   获取锁的 Key。
    /// </summary>
    /// <returns></returns>
    string GetLockKey();
}