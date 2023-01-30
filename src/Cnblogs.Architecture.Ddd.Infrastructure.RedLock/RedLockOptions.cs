namespace Cnblogs.Architecture.Ddd.Infrastructure.RedLock;

/// <summary>
///     分布式锁配置。
/// </summary>
public class RedLockOptions
{
    /// <summary>
    ///     Redis Host。
    /// </summary>
    public string Host { get; set; } = "redis";

    /// <summary>
    ///     Redis 端口。
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    ///     Redis 密码。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    ///     同步时间，以毫秒为单位。
    /// </summary>
    public int SyncTimeout { get; set; } = 2000;

    /// <summary>
    ///     锁默认过期时间，以毫秒为单位。
    /// </summary>
    public int Expiry { get; set; } = 20 * 1000;

    /// <summary>
    ///     等待锁的最大时间，以毫秒为单位。
    /// </summary>
    public int Wait { get; set; } = 3000;

    /// <summary>
    ///     重试间隔，以毫秒为单位。
    /// </summary>
    public int Retry { get; set; } = 1000;

    /// <summary>
    ///     获得 Redis 连接字符串。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Host 为空时抛出。</exception>
    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(Host))
        {
            throw new ArgumentNullException(nameof(Host));
        }

        var connectionString = $"{Host}:{Port}";
        if (!string.IsNullOrEmpty(Password))
        {
            connectionString += $",password={Password},syncTimeout={SyncTimeout}";
        }

        return connectionString;
    }
}