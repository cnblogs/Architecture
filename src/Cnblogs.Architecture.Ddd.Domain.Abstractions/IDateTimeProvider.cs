namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     时间生成器。
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    ///     获取当前时间。
    /// </summary>
    /// <returns>当前时间。</returns>
    DateTimeOffset Now();

    /// <summary>
    ///     获取当前距离 1970-01-01T00:00:00Z 的秒数。
    /// </summary>
    /// <returns></returns>
    long UnixSeconds();
}