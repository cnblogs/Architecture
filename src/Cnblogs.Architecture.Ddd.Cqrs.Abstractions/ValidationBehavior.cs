using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Validate requests that implements <see cref="IValidatable" />.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
        _logger.LogInformation("----- Validating request {RequestType}", request.GetType().Name);
        var error = request.Validate();
        if (error is null)
        {
            return await next();
        }

        _logger.LogWarning(
            "----- Validation failed with error, type: {RequestType}, Request: {Request}, Message: {Message}",
            request.GetType().Name,
            request,
            error.Message);

        return new TResponse
        {
            IsValidationError = true,
            ErrorMessage = error.Message,
            ValidationError = error
        };
    }
}