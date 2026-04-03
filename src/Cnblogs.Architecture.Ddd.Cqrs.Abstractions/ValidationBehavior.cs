using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Validate requests that implements <see cref="IValidatable" />.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public partial class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IValidatable, IRequest<TResponse>
    where TResponse : IValidationResponse, new()
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     Create a new <see cref="ValidationBehavior{TRequest,TResponse}" />.
    /// </summary>
    /// <param name="logger">The log provider.</param>
    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        LogValidating(request.GetType().Name);
        var errors = new ValidationErrors();
        request.Validate(errors);
        if (errors.Count == 0)
        {
            return await next(cancellationToken);
        }

        LogValidationFailedWithError(request.GetType().Name, request, errors.First().Message);

        return new TResponse
        {
            IsValidationError = true,
            ErrorMessage = errors.First().Message,
            ValidationErrors = errors
        };
    }

    [LoggerMessage(LogLevel.Information, "----- Validating request {RequestType}")]
    partial void LogValidating(string requestType);

    [LoggerMessage(LogLevel.Warning, "----- Validation failed with error, type: {RequestType}, Request: {Request}, Message: {Message}")]
    partial void LogValidationFailedWithError(string requestType, TRequest request, string message);
}
