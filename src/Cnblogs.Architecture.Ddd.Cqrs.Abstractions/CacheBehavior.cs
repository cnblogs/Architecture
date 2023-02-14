namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Options for handing <see cref="ICachableRequest"/>.
/// </summary>
public enum CacheBehavior
{
    /// <summary>
    ///     Update cache after cache missed, this is the default behavior.
    /// </summary>
    UpdateCacheIfMiss = 1,

    /// <summary>
    ///     Do not cache this request.
    /// </summary>
    DisabledCache = 2
}