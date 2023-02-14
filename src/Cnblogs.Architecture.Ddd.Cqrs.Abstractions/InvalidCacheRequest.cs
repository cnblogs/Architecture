using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents request to invalid caches.
/// </summary>
/// <param name="Request">The request that been cached.</param>
/// <param name="InvalidWholeGroup">Invalid cache for the group that <paramref name="Request"/> was in.</param>
/// <param name="ThrowIfFailed">Throw exceptions if fails, overrides same options in <see cref="CacheableRequestOptions"/>.</param>
public record InvalidCacheRequest(
    ICachableRequest Request,
    bool InvalidWholeGroup = false,
    bool? ThrowIfFailed = null) : IRequest;