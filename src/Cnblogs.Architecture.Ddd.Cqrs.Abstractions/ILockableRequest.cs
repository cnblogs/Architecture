namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a request that needs distributed locks.
/// </summary>
public interface ILockableRequest
{
    /// <summary>
    ///     Get the key of distributed lock.
    /// </summary>
    /// <returns>The key of distributed lock.</returns>
    string GetLockKey();
}