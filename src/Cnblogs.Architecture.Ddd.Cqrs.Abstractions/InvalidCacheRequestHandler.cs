using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     The default handler for <see cref="InvalidCacheRequest"/>.
/// </summary>
public partial class InvalidCacheRequestHandler(
    ICacheProvider cacheProvider,
    ILogger<InvalidCacheRequestHandler> logger)
    : IRequestHandler<InvalidCacheRequest>, IRequestHandler<InvalidCacheGroupsRequest>
{
    /// <inheritdoc />
    public async Task Handle(InvalidCacheRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = request.Request.CacheKey();
            var groupKey = request.Request.CacheGroupKeys();
            await cacheProvider.RemoveAsync(cacheKey, cancellationToken);

            if (groupKey is not null && request.InvalidWholeGroup)
            {
                await cacheProvider.RemoveGroupAsync(groupKey, cancellationToken);
            }
        }
        catch (Exception e)
        {
            LogInvalidCacheFailed(request.GetType().Name, request, e.Message);
            if (request.ThrowIfFailed == true)
            {
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task Handle(InvalidCacheGroupsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await cacheProvider.RemoveGroupAsync(request.GroupKeys, cancellationToken);
        }
        catch (Exception e)
        {
            LogInvalidCacheFailed(request.GetType().Name, request, e.Message);
            if (request.ThrowIfFailed == true)
            {
                throw;
            }
        }
    }

    [LoggerMessage(
        LogLevel.Error,
        "----- Invalid Cache Failed, Type: {TypeName}, Request: {RequestBody}, Message: {Message}")]
    partial void LogInvalidCacheFailed(string typeName, InvalidCacheRequest requestBody, string message);

    [LoggerMessage(
        LogLevel.Error,
        "----- Invalid Cache Failed, Type: {TypeName}, Request: {RequestBody}, Message: {Message}")]
    partial void LogInvalidCacheFailed(string typeName, InvalidCacheGroupsRequest requestBody, string message);
}
