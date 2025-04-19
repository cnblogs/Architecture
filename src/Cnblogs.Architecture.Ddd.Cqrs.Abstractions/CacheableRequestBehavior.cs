using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Handler for <see cref="ICachableRequest" />.
/// </summary>
/// <typeparam name="TRequest">Request that implements <see cref="ICachableRequest" />.</typeparam>
/// <typeparam name="TResponse">Cached result for <typeparamref name="TRequest" />.</typeparam>
public class CacheableRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachableRequest, IRequest<TResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILocalCacheProvider? _local;
    private readonly IRemoteCacheProvider? _remote;
    private readonly CacheableRequestOptions _options;
    private readonly ILogger<CacheableRequestBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     Create <see cref="CacheableRequestBehavior{TRequest,TResponse}" />.
    /// </summary>
    /// <param name="providers">Cache providers.</param>
    /// <param name="dateTimeProvider">Datetime provider.</param>
    /// <param name="options">Options for cache behavior.</param>
    /// <param name="logger">logger.</param>
    public CacheableRequestBehavior(
        IEnumerable<ICacheProvider> providers,
        IDateTimeProvider dateTimeProvider,
        IOptions<CacheableRequestOptions> options,
        ILogger<CacheableRequestBehavior<TRequest, TResponse>> logger)
    {
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _logger = logger;
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
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is
            {
                LocalCacheBehavior: CacheBehavior.DisabledCache,
                RemoteCacheBehavior: CacheBehavior.DisabledCache
            })
        {
            // cache disabled
            return await next(cancellationToken);
        }

        CacheEntry<TResponse>? result = null;
        var cacheKey = request.CacheKey();
        var cacheGroupKey = request.CacheGroupKey();
        if (request.LocalCacheBehavior is not CacheBehavior.DisabledCache)
        {
            result = await GetCacheEntryAsync(_local, cacheGroupKey, cacheKey);
        }

        if (result is null && request.RemoteCacheBehavior is not CacheBehavior.DisabledCache)
        {
            result = await GetCacheEntryAsync(_remote, cacheGroupKey, cacheKey);
        }

        if (result != null)
        {
            return result.Value;
        }

        result = new CacheEntry<TResponse>(await next(cancellationToken), _dateTimeProvider.Now().ToUnixTimeSeconds());

        if (request.LocalCacheBehavior is CacheBehavior.UpdateCacheIfMiss)
        {
            await UpdateCacheEntryAsync(_local, cacheKey, result.Value, request.LocalExpires);
        }

        if (request.RemoteCacheBehavior is CacheBehavior.UpdateCacheIfMiss)
        {
            await UpdateCacheEntryAsync(_remote, cacheKey, result.Value, request.RemoteExpires);
        }

        return result.Value;
    }

    private async Task<CacheEntry<TResponse>?> GetCacheEntryAsync(
        ICacheProvider? provider,
        string? cacheGroupKey,
        string cacheKey)
    {
        if (provider is null)
        {
            return null;
        }

        try
        {
            var cacheValue = await provider.GetAsync<TResponse>(cacheKey);
            if (cacheValue is null)
            {
                return null;
            }

            if (cacheGroupKey is not null)
            {
                var timestamp = await provider.GetAsync<long>(cacheGroupKey);
                if (timestamp is null)
                {
                    var current = _dateTimeProvider.Now().ToUnixTimeSeconds();
                    timestamp = new CacheEntry<long>(current, current);
                    await provider.UpdateAsync(cacheGroupKey, timestamp);
                }

                if (timestamp.Value > cacheValue.TimestampInSeconds)
                {
                    // cache is invalid
                    return null;
                }
            }

            return cacheValue;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "----- Get entry from cache failed, provider: {Provider}, key: {CacheKey}, groupKey: {CacheGroupKey}, error: {Error}",
                provider.GetType().Name,
                cacheKey,
                cacheGroupKey,
                e.Message);
            if (_options.ThrowIfFailedOnGet)
            {
                throw;
            }

            return null;
        }
    }

    private async Task UpdateCacheEntryAsync(
        ICacheProvider? provider,
        string cacheKey,
        TResponse value,
        TimeSpan? expires)
    {
        if (provider is null)
        {
            return;
        }

        try
        {
            if (expires.HasValue)
            {
                await provider.UpdateAsync(cacheKey, value, expires.Value);
            }
            else
            {
                await provider.UpdateAsync(cacheKey, value);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                "----- Update cache failed, provider: {Provider}, key: {CacheKey}, value: {Value}, error: {Error}",
                provider.GetType().Name,
                cacheKey,
                value,
                e.Message);
            if (_options.ThrowIfFailedOnUpdate)
            {
                throw;
            }
        }
    }
}
