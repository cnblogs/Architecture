namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     默认的时间生成器。
/// </summary>
public class DefaultDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset Now()
    {
        return DateTimeOffset.Now;
    }

    /// <inheritdoc />
    public long UnixSeconds()
    {
        return Now().ToUnixTimeSeconds();
    }
}