using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Handler for <see cref="ICachableRequest" />.
/// </summary>
/// <typeparam name="TRequest">Request that implements <see cref="ICachableRequest" />.</typeparam>
/// <typeparam name="TResponse">Cached result for <typeparamref name="TRequest" />.</typeparam>
public class CacheableRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachableRequest, IRequest<TResponse>
{
    private readonly ICacheProvider _cache;

    /// <summary>
    ///     Create <see cref="CacheableRequestBehavior{TRequest,TResponse}" />.
    /// </summary>
    /// <param name="cache">Cache providers.</param>
    public CacheableRequestBehavior(ICacheProvider cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is
            {
                LocalCacheBehavior: CacheBehavior.DisabledCache, RemoteCacheBehavior: CacheBehavior.DisabledCache
            })
        {
            // cache disabled
            return await next(cancellationToken);
        }

        var cacheKey = request.CacheKey();
        var cacheGroupKey = request.CacheGroupKey();
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token => await next(token),
            request.RemoteCacheBehavior == CacheBehavior.DisabledCache ? null : request.RemoteExpires,
            request.LocalCacheBehavior == CacheBehavior.DisabledCache ? null : request.LocalExpires,
            cacheGroupKey,
            cancellationToken);
    }
}
