using IdGen;

namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// Default ID provider implementation
/// </summary>
public class DefaultIdProvider(IDateTimeProvider dateTimeProvider, DefaultIdProviderOption option) : IIdProvider
{
    private readonly int[] _shuffledTails = Enumerable.Range(0, 100).Shuffle().ToArray();
    private readonly int[] _eigenCounts = new int[1000];
    private readonly Lock _secondResetLock = new();

    private readonly IdGenerator _snowflakeGenerator = new(
        option.InstanceId,
        new IdGeneratorOptions(
            timeSource: new DefaultTimeSource(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            sequenceOverflowStrategy: option.IdGenStrategy));

    private long _currentSecond;

    /// <inheritdoc />
    public long NextReadable()
    {
        return NextReadable(option.InstanceId);
    }

    /// <inheritdoc />
    public long NextReadable(int eigen)
    {
        eigen = Math.Abs(eigen % 1000);
        var now = dateTimeProvider.Now();
        var secondKey = long.Parse(now.ToString("yyyyMMddHHmmss"));
        var time = secondKey * 100000;

        EnsureSecondReset(secondKey);

        var concurrency = Interlocked.Increment(ref _eigenCounts[eigen]);
        if (concurrency > 100)
        {
            return HandleOverflow(eigen);
        }

        return time + eigen * 100 + _shuffledTails[(concurrency - 1) % _shuffledTails.Length];
    }

    /// <inheritdoc />
    public long NextNumeric()
    {
        return _snowflakeGenerator.CreateId();
    }

    private void EnsureSecondReset(long secondKey)
    {
        if (Volatile.Read(ref _currentSecond) == secondKey)
        {
            return;
        }

        lock (_secondResetLock)
        {
            if (_currentSecond == secondKey)
            {
                return;
            }

            Array.Clear(_eigenCounts);
            Volatile.Write(ref _currentSecond, secondKey);
        }
    }

    private long HandleOverflow(int eigen)
    {
        if (option.WhenSequenceOverflow == SequenceOverflowStrategy.SpinWait)
        {
            SpinWait.SpinUntil(() =>
            {
                var now = dateTimeProvider.Now();
                return long.Parse(now.ToString("yyyyMMddHHmmss")) != Volatile.Read(ref _currentSecond);
            });

            return NextReadable(eigen);
        }

        throw new InvalidOperationException(
            $"Sequence overflow for eigen {eigen}: more than 100 IDs generated in one second.");
    }
}
