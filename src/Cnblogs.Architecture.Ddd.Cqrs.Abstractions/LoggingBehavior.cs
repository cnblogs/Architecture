using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Middleware for logging requests and events.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     Create a new <see cref="LoggingBehavior{TRequest, TResponse}" /> instance.
    /// </summary>
    /// <param name="logger">Log provider.</param>
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
        LogHandlingRequest(request);
        var result = await next(cancellationToken);
        LogHandledRequest(request);
        return result;
    }

    [LoggerMessage(LogLevel.Debug, "Handling {request}")]
    partial void LogHandlingRequest(TRequest request);

    [LoggerMessage(LogLevel.Debug, "Handled {Request}")]
    partial void LogHandledRequest(TRequest request);
}
