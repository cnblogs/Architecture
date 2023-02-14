namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Options for handling <see cref="ICachableRequest"/>.
/// </summary>
public class CacheableRequestOptions
{
    /// <summary>
    ///     Rethrow exception if getting cached result failed.
    /// </summary>
    public bool ThrowIfFailedOnGet { get; set; }

    /// <summary>
    ///     Rethrow exception if updating cache failed.
    /// </summary>
    public bool ThrowIfFailedOnUpdate { get; set; }

    /// <summary>
    ///     Rethrow exception if removing cache failed, this option can be overriden by <see cref="InvalidCacheRequest.ThrowIfFailed"/> for specific type of request.
    /// </summary>
    public bool ThrowIfFailedOnRemove { get; set; }
}