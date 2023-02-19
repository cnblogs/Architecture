using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     The default handler for <see cref="InvalidCacheRequest"/>.
/// </summary>
public class InvalidCacheRequestHandler : IRequestHandler<InvalidCacheRequest>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILocalCacheProvider? _local;
    private readonly IRemoteCacheProvider? _remote;
    private readonly CacheableRequestOptions _options;
    private readonly ILogger<InvalidCacheRequestHandler> _logger;

    /// <summary>
    ///     Create a <see cref="CacheableRequestBehavior{TRequest,TResponse}" />.
    /// </summary>
    /// <param name="providers">Cache providers.</param>
    /// <param name="dateTimeProvider">Datetime providers.</param>
    /// <param name="options">Cache options.</param>
    /// <param name="logger">log provider.</param>
    public InvalidCacheRequestHandler(
        IEnumerable<ICacheProvider> providers,
        IDateTimeProvider dateTimeProvider,
        IOptions<CacheableRequestOptions> options,
        ILogger<InvalidCacheRequestHandler> logger)
    {
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _options = options.Value;
        foreach (var provider in providers)
        {
            switch (provider)
            {
                case ILocalCacheProvider local:
                    _local = local;
                    break;
                case IRemoteCacheProvider remote:
                    _remote = remote;
                    break;
            }
        }

        if (_local is null && _remote is null)
        {
            throw new InvalidOperationException(
                "no cache provider is available for use, check if you injected any ICacheProvider that implements ILocalCacheProvider or IRemoteCacheProvider to DI container");
        }
    }

    /// <inheritdoc />
    public async Task Handle(InvalidCacheRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = request.Request.CacheKey();
            var groupKey = request.Request.CacheGroupKey();
            if (request.Request.LocalCacheBehavior is not CacheBehavior.DisabledCache
                && _local is not null)
            {
                await _local.RemoveAsync(cacheKey);
            }

            if (request.Request.RemoteCacheBehavior is not CacheBehavior.DisabledCache
                && _remote is not null)
            {
                await _remote.RemoveAsync(cacheKey);
            }

            if (groupKey is not null && request.InvalidWholeGroup)
            {
                await (_local?.UpdateAsync(groupKey, _dateTimeProvider.Now().ToUnixTimeSeconds())
                       ?? Task.CompletedTask);
                await (_remote?.UpdateAsync(groupKey, _dateTimeProvider.Now().ToUnixTimeSeconds())
                       ?? Task.CompletedTask);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                "----- Invalid Cache Failed, Type: {TypeName}, Request: {RequestBody}, Message: {Message}",
                request.GetType().Name,
                request,
                e.Message);
            if (request.ThrowIfFailed ?? _options.ThrowIfFailedOnRemove)
            {
                throw;
            }
        }
    }
}
