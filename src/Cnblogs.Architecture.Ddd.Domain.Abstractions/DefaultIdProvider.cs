using IdGen;

namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// Default ID provider implementation
/// </summary>
public class DefaultIdProvider(IDateTimeProvider dateTimeProvider, int id) : IIdProvider
{
    private readonly int[] _shuffledTails = Enumerable.Range(0, 100).Shuffle().ToArray();

    private readonly IdGenerator _snowflakeGenerator = new(
        id,
        new IdGeneratorOptions(
            timeSource: new DefaultTimeSource(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            sequenceOverflowStrategy: SequenceOverflowStrategy.SpinWait));

    private uint _seq;

    /// <inheritdoc />
    public long NextReadable()
    {
        return NextReadable(id);
    }

    /// <inheritdoc />
    public long NextReadable(int eigen)
    {
        eigen = Math.Abs(eigen % 1000);
        var time = long.Parse(dateTimeProvider.Now().ToString("yyyyMMddHHmmss")) * 100000;
        var seq = Interlocked.Increment(ref _seq);
        return time
               + eigen * 100
               + _shuffledTails[seq % _shuffledTails.Length];
    }

    /// <inheritdoc />
    public long NextNumeric()
    {
        return _snowflakeGenerator.CreateId();
    }
}
