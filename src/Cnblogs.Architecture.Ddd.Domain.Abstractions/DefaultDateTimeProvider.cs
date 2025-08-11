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
    public DateTimeOffset Yesterday()
    {
        return Today().AddDays(-1);
    }

    /// <inheritdoc />
    public DateTimeOffset EndOfYesterday()
    {
        return Today().AddDays(-1).EndOfTheDay();
    }

    /// <inheritdoc />
    public DateTimeOffset Today()
    {
        var now = Now();
        return new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
    }

    /// <inheritdoc />
    public DateTimeOffset EndOfToday()
    {
        return Today().EndOfTheDay();
    }

    /// <inheritdoc />
    public DateTimeOffset Tomorrow()
    {
        return Today().AddDays(1);
    }

    /// <inheritdoc />
    public DateTimeOffset EndOfTomorrow()
    {
        return Today().AddDays(1).EndOfTheDay();
    }

    /// <inheritdoc />
    public long UnixSeconds()
    {
        return Now().ToUnixTimeSeconds();
    }

    /// <inheritdoc />
    public long UnixMilliseconds()
    {
        return Now().ToUnixTimeMilliseconds();
    }
}
