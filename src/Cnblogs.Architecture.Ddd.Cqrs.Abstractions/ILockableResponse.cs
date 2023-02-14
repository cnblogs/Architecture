namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents response for <see cref="ILockableRequest"/>.
/// </summary>
public interface ILockableResponse
{
    /// <summary>
    ///     Indicates whether lock was required successfully.
    /// </summary>
    bool IsConcurrentError { get; init; }

    /// <summary>
    ///    Indicates whether lock was required.
    /// </summary>
    bool LockAcquired { get; set; }
}