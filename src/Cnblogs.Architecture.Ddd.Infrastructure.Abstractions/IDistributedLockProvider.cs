namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     分布式锁提供器。
/// </summary>
public interface IDistributedLockProvider
{
    /// <summary>
    ///     加锁并执行操作。
    /// </summary>
    /// <param name="key">锁的 Id。</param>
    /// <param name="action">要执行的操作。</param>
    /// <param name="expiresIn">锁的过期时间。</param>
    /// <typeparam name="T"><paramref name="action" /> 的返回值。</typeparam>
    /// <exception cref="AcquireDistributionLockFailedException">当获取不到锁时会抛出这个异常。</exception>
    /// <returns></returns>
    Task<T> ExecuteWithLockAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiresIn);
}