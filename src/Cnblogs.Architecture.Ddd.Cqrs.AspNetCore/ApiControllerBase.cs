using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     A base class for an API controller with methods that return <see cref="IActionResult"/> based ons <see cref="CommandResponse{TError}"/>.
/// </summary>
[ApiController]
public class ApiControllerBase : ControllerBase
{
    private CqrsHttpOptions? _cqrsHttpOptions;

    private CqrsHttpOptions CqrsHttpOptions
    {
        get
        {
            _cqrsHttpOptions ??= HttpContext.RequestServices.GetRequiredService<IOptions<CqrsHttpOptions>>().Value;
            return _cqrsHttpOptions;
        }
    }

    /// <summary>
    ///     Handle command response and return 204 if success, 400 if error.
    /// </summary>
    /// <param name="response">The command response.</param>
    /// <typeparam name="TError">The type of error.</typeparam>
    /// <returns><see cref="IActionResult"/></returns>
    protected IActionResult HandleCommandResponse<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        if (response.IsSuccess())
        {
            return NoContent();
        }

        return HandleErrorCommandResponse(response);
    }

    /// <summary>
    ///     Handle command response and return 200 if success, 400 if error.
    /// </summary>
    /// <param name="response">The command response.</param>
    /// <typeparam name="TResponse">The response type when success.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <returns><see cref="IActionResult"/></returns>
    protected IActionResult HandleCommandResponse<TResponse, TError>(CommandResponse<TResponse, TError> response)
        where TError : Enumeration
    {
        if (response.IsSuccess())
        {
            return Request.Headers.CqrsVersion() > 1 ? Ok(response) : Ok(response.Response);
        }

        return HandleCommandResponse((CommandResponse<TError>)response);
    }

    private IActionResult HandleErrorCommandResponse<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        var errorResponseType = CqrsHttpOptions.CommandErrorResponseType;
        if (Request.Headers.Accept.Contains("application/cqrs") || Request.Headers.CqrsVersion() > 1)
        {
            errorResponseType = ErrorResponseType.Cqrs;
        }

        return errorResponseType switch
        {
            ErrorResponseType.PlainText => MapErrorCommandResponseToPlainText(response),
            ErrorResponseType.ProblemDetails => MapErrorCommandResponseToProblemDetails(response),
            ErrorResponseType.Cqrs => MapErrorCommandResponseToCqrsResponse(response),
            ErrorResponseType.Custom => CustomErrorCommandResponseMap(response),
            _ => throw new ArgumentOutOfRangeException(
                $"Unsupported CommandErrorResponseType: {CqrsHttpOptions.CommandErrorResponseType}")
        };
    }

    /// <summary>
    ///     Provides custom map logic that mapping error <see cref="CommandResponse{TError}"/> to <see cref="IActionResult"/> when <see cref="CqrsHttpOptions.CommandErrorResponseType"/> is <see cref="ErrorResponseType.Custom"/>.
    ///     The <c>CqrsHttpOptions.CustomCommandErrorResponseMapper</c> will be used as default implementation if configured. PlainText mapper will be used as the final fallback.
    /// </summary>
    /// <param name="response">The <see cref="CommandResponse{TError}"/> in error state.</param>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <returns></returns>
    protected virtual IActionResult CustomErrorCommandResponseMap<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        if (CqrsHttpOptions.CustomCommandErrorResponseMapper != null)
        {
            var result = CqrsHttpOptions.CustomCommandErrorResponseMapper.Invoke(response, HttpContext);
            return new HttpActionResult(result);
        }

        return MapErrorCommandResponseToPlainText(response);
    }

    private IActionResult MapErrorCommandResponseToCqrsResponse<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return StatusCode(429);
        }

        return BadRequest(response);
    }

    private IActionResult MapErrorCommandResponseToProblemDetails<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        if (response.IsValidationError)
        {
            foreach (var (message, parameterName) in response.ValidationErrors)
            {
                ModelState.AddModelError(parameterName ?? "command", message);
            }

            return ValidationProblem();
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return Problem(
                "The lock can not be acquired within time limit, please try later.",
                null,
                429,
                "Concurrent error");
        }

        return Problem(response.GetErrorMessage(), null, 400, "Execution failed");
    }

    private IActionResult MapErrorCommandResponseToPlainText<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        if (response.IsValidationError)
        {
            return BadRequest(string.Join('\n', response.ValidationErrors.Select(x => x.Message)));
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return StatusCode(429);
        }

        return BadRequest(response.ErrorCode?.Name ?? response.ErrorMessage);
    }

    private static IActionResult BadRequest(string text)
    {
        return new ContentResult
        {
            Content = text,
            ContentType = "text/plain",
            StatusCode = 400
        };
    }
}
