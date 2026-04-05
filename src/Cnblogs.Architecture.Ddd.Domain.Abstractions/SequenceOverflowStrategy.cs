namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// Strategy when overflowed
/// </summary>
public enum SequenceOverflowStrategy
{
    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> when id overflow
    /// </summary>
    Throw = 0,

    /// <summary>
    /// Wait for next tick then generate id.
    /// </summary>
    SpinWait = 1
}
