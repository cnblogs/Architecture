namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     默认的随机数生成器。
/// </summary>
public class DefaultRandomProvider : IRandomProvider
{
    /// <inheritdoc />
    public int Next(int max)
    {
        return new Random().Next(max);
    }

    /// <inheritdoc />
    public int Next(int min, int max)
    {
        return new Random().Next(min, max);
    }

    /// <inheritdoc />
    public double Next(double max)
    {
        if (max < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(max), max, "max must be positive or equal to 0");
        }

        return new Random().NextDouble() * max;
    }
}