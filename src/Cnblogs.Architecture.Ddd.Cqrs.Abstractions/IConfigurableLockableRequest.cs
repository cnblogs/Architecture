namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Definitions of a <see cref="ILockableRequest"/> with some configurations.
/// </summary>
public interface IConfigurableLockableRequest : ILockableRequest
{
    /// <summary>
    ///     The maximum waiting time for requiring a lock.
    /// </summary>
    TimeSpan ExpiresIn { get; }
}