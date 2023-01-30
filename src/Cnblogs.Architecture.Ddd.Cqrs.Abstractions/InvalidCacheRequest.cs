using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     清除缓存请求。
/// </summary>
public record InvalidCacheRequest(
    ICacheableRequest Request,
    bool InvalidWholeGroup = false,
    bool? ThrowIfFailed = null) : IRequest;