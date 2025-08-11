namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     Provides system date time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    ///     Get current time.
    /// </summary>
    /// <returns>Current time.</returns>
    DateTimeOffset Now();

    /// <summary>
    ///     Get <see cref="DateTimeOffset"/> of yesterday's date.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset Yesterday();

    /// <summary>
    ///     Get <see cref="DateTimeOffset"/> of yesterday's date with time set to 23:59:59.999.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset EndOfYesterday();

    /// <summary>
    ///     Get <see cref="DateTimeOffset"/> of today's date.
    /// </summary>
    /// <returns>Today's date.</returns>
    DateTimeOffset Today();

    /// <summary>
    ///     Get <see cref="DateTimeOffset"/> of today's date with time set to 23:59:59.999.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset EndOfToday();

    /// <summary>
    ///     Get <see cref="DateTimeOffset"/> of tomorrow's date with time set to 0:00:00.000.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset Tomorrow();

    /// <summary>
    ///     Get <see cref="DateTimeOffset"/> of tomorrow's date with time set to 23:59:59.000.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset EndOfTomorrow();

    /// <summary>
    ///     Get number of seconds that have elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
    long UnixSeconds();

    /// <summary>
    ///     Get number of milliseconds that have elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <returns></returns>
    long UnixMilliseconds();
}
