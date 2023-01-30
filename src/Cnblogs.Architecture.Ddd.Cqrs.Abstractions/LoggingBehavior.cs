using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     记录命令/查询日志
/// </summary>
/// <typeparam name="TRequest">请求类型。</typeparam>
/// <typeparam name="TResponse">返回类型。</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     新建一个 <see cref="LoggingBehavior{TRequest, TResponse}" /> 类型的实例。
    /// </summary>
    /// <param name="logger">日志记录器。</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling {@Request}", request);
        var result = await next();
        _logger.LogDebug("Handled {@Request}", request);
        return result;
    }
}