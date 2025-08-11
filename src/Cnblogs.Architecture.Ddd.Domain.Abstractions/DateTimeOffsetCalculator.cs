namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// Handy calculator for DateTime
/// </summary>
public static class DateTimeOffsetCalculator
{
    /// <summary>
    /// Get a new <see cref="DateTimeOffset"/> with time set to 0:00:00
    /// </summary>
    /// <param name="dateTime">Input <see cref="DateTimeOffset"/>.</param>
    /// <returns></returns>
    public static DateTimeOffset StartOfTheDay(this DateTimeOffset dateTime)
    {
        return new DateTimeOffset(dateTime.Date, dateTime.Offset);
    }

    /// <summary>
    /// Get a new <see cref="DateTimeOffset"/> with time set to 23:59:59.999.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset EndOfTheDay(this DateTimeOffset dateTime)
    {
        return StartOfTheDay(dateTime).AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Get a new <see cref="DateTimeOffset"/> that set the start of a week for the given date.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset StartOfTheWeek(this DateTimeOffset dateTime)
    {
        var monday = dateTime.DayOfWeek switch
        {
            DayOfWeek.Monday => dateTime,
            DayOfWeek.Sunday => dateTime.AddDays(-6),
            _ => dateTime.AddDays(-(int)(dateTime.DayOfWeek - 1))
        };

        return monday.StartOfTheDay();
    }

    /// <summary>
    /// Get a new <see cref="DateTimeOffset"/> that set to the end of a week for the given date.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset EndOfTheWeek(this DateTimeOffset dateTime)
    {
        var sunday = dateTime.DayOfWeek switch
        {
            DayOfWeek.Sunday => dateTime,
            DayOfWeek.Monday => dateTime.AddDays(6),
            _ => dateTime.AddDays((int)(7 - dateTime.DayOfWeek))
        };

        return sunday.EndOfTheDay();
    }

    /// <summary>
    /// Check whether two <see cref="DateTimeOffset"/> is in same date.
    /// </summary>
    /// <param name="left">First <see cref="DateTimeOffset"/> to check.</param>
    /// <param name="right">Second <see cref="DateTimeOffset"/> to check.</param>
    /// <returns></returns>
    public static bool IsInSameDate(this DateTimeOffset left, DateTimeOffset right)
    {
        return left.Year == right.Year && left.DayOfYear == right.DayOfYear;
    }
}
