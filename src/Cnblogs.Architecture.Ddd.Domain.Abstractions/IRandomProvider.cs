namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     随机数生成器。
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    ///     指定范围内生成随机数。
    /// </summary>
    /// <param name="max">随机数的上界，必须大于等于零，返回值将小于它。</param>
    /// <returns>小于 <paramref name="max" /> 的随机数。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> 小于 0。</exception>
    public int Next(int max);

    /// <summary>
    ///     指定范围（左闭右开）内生成随机数。
    /// </summary>
    /// <param name="min">随机数的下界。</param>
    /// <param name="max">随机数的上界。</param>
    /// <returns>大于等于 <paramref name="min" /> 且小于 <paramref name="max" /> 的随机数。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="min" /> 大于 <paramref name="max" />。</exception>
    public int Next(int min, int max);

    /// <summary>
    ///     指定范围内生成随机数。
    /// </summary>
    /// <param name="max">随机数的上界，必须大于等于零，返回值将小于它。</param>
    /// <returns>小于 <paramref name="max" /> 的随机数。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> 小于 0。</exception>
    public double Next(double max);
}