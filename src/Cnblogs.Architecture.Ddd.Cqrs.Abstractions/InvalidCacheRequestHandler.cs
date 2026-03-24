using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     The default handler for <see cref="InvalidCacheRequest"/>.
/// </summary>
public class InvalidCacheRequestHandler : IRequestHandler<InvalidCacheRequest>
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<InvalidCacheRequestHandler> _logger;

    /// <summary>
    ///     Create a <see cref="CacheableRequestBehavior{TRequest,TResponse}" />.
    /// </summary>
    /// <param name="cacheProvider">Cache providers.</param>
    /// <param name="logger">log provider.</param>
    public InvalidCacheRequestHandler(
        ICacheProvider cacheProvider,
        ILogger<InvalidCacheRequestHandler> logger)
    {
        _cacheProvider = cacheProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(InvalidCacheRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = request.Request.CacheKey();
            var groupKey = request.Request.CacheGroupKey();
            await _cacheProvider.RemoveAsync(cacheKey, cancellationToken);

            if (groupKey is not null && request.InvalidWholeGroup)
            {
                await _cacheProvider.RemoveGroupAsync(groupKey, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                "----- Invalid Cache Failed, Type: {TypeName}, Request: {RequestBody}, Message: {Message}",
                request.GetType().Name,
                request,
                e.Message);
            if (request.ThrowIfFailed == true)
            {
                throw;
            }
        }
    }
}
