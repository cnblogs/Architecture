using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Handle requests that require distributed lock.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public partial class LockableRequestBehavior<TRequest, TResponse>(
    IDistributedLockProvider distributedLockProvider,
    ILogger<LockableRequestBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ILockableRequest, IRequest<TResponse>
    where TResponse : ILockableResponse, new()
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            TimeSpan? expiresIn = request is IConfigurableLockableRequest configurableLockableRequest
                ? configurableLockableRequest.ExpiresIn
                : null;
            var response = await distributedLockProvider.ExecuteWithLockAsync(
                request.GetLockKey(),
                async () => await next(cancellationToken),
                expiresIn);
            response.LockAcquired = true;
            return response;
        }
        catch (AcquireDistributionLockFailedException e)
        {
            LogAcquireDistributionLockFailed(logger, request, e.LockKey, e);
            return new TResponse { IsConcurrentError = true, LockAcquired = false };
        }
    }

    [LoggerMessage(LogLevel.Error, "Acquire Distribution Lock Failed, Request: {@Request}, LockKey: {LockLey}")]
    partial void LogAcquireDistributionLockFailed(
        ILogger logger,
        TRequest request,
        string lockLey,
        AcquireDistributionLockFailedException acquireDistributionLockFailedException);
}
