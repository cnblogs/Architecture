using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Microsoft.Extensions.Options;

using RedLockNet.SERedis;

namespace Cnblogs.Architecture.Ddd.Infrastructure.RedLock;

/// <summary>
///     使用 RedLock 实现分布式锁。
/// </summary>
public class RedLockDistributionLockProvider : IDistributedLockProvider
{
    private readonly RedLockFactory _lockFactory;
    private readonly RedLockOptions _options;

    /// <summary>
    ///     构建一个 RedLock 分布式锁提供程序。
    /// </summary>
    /// <param name="lockFactory"> RedLock 工厂类。</param>
    /// <param name="options">配置信息。</param>
    public RedLockDistributionLockProvider(RedLockFactory lockFactory, IOptions<RedLockOptions> options)
    {
        _lockFactory = lockFactory;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<T> ExecuteWithLockAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiresIn)
    {
        expiresIn ??= TimeSpan.FromMilliseconds(_options.Expiry);
        var wait = TimeSpan.FromMilliseconds(_options.Wait);
        var retry = TimeSpan.FromMilliseconds(_options.Retry);
        await using var redLock = await _lockFactory.CreateLockAsync(key, expiresIn.Value, wait, retry);
        if (redLock.IsAcquired == false)
        {
            throw new AcquireDistributionLockFailedException(key);
        }

        return await action();
    }
}