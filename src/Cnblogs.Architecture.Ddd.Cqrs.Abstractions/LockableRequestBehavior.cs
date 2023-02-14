using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Handle requests that require distributed lock.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class LockableRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ILockableRequest, IRequest<TResponse>
    where TResponse : ILockableResponse, new()
{
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly ILogger<LockableRequestBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     Create a new <see cref="LockableRequestBehavior{TRequest, TResponse}" /> instance.
    /// </summary>
    /// <param name="distributedLockProvider">Distributed lock provider.</param>
    /// <param name="logger">log provider.</param>
    public LockableRequestBehavior(
        IDistributedLockProvider distributedLockProvider,
        ILogger<LockableRequestBehavior<TRequest, TResponse>> logger)
    {
        _distributedLockProvider = distributedLockProvider;
        _logger = logger;
    }

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
            var response = await _distributedLockProvider.ExecuteWithLockAsync(
                request.GetLockKey(),
                async () => await next(),
                expiresIn);
            response.LockAcquired = true;
            return response;
        }
        catch (AcquireDistributionLockFailedException e)
        {
            _logger.LogError(
                e,
                "Acquire Distribution Lock Failed, Request: {@Request}, LockKey: {@LockLey}",
                request,
                e.LockKey);
            return new TResponse { IsConcurrentError = true, LockAcquired = false };
        }
    }
}