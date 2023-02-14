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
    ///     Get <see cref="DateTimeOffset"/> today's date.
    /// </summary>
    /// <returns>Today's date.</returns>
    DateTimeOffset Today();

    /// <summary>
    ///     Get number of seconds that have elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
    long UnixSeconds();
}
