using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     处理需要分布式锁的请求。
/// </summary>
/// <typeparam name="TRequest">请求类型。</typeparam>
/// <typeparam name="TResponse">响应类型。</typeparam>
public class LockableRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ILockableRequest, IRequest<TResponse>
    where TResponse : ILockableResponse, new()
{
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly ILogger<LockableRequestBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     创建一个新的 <see cref="LockableRequestBehavior{TRequest, TResponse}" /> 实例。
    /// </summary>
    /// <param name="distributedLockProvider">分布式锁提供器。</param>
    /// <param name="logger">日志记录器。</param>
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