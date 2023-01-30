using MediatR;

using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     对实现了 <see cref="IValidatable" /> 的 <see cref="IRequest" /> 进行验证。
/// </summary>
/// <typeparam name="TRequest">请求类型。</typeparam>
/// <typeparam name="TResponse">结果类型。</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IValidatable, IRequest<TResponse>
    where TResponse : IValidationResponse, new()
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     构造一个 <see cref="ValidationBehavior{TRequest,TResponse}" />。
    /// </summary>
    /// <param name="logger"></param>
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