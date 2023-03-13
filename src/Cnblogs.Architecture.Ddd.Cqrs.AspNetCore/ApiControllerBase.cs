using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     A base class for an API controller with methods that return <see cref="IActionResult"/> based ons <see cref="CommandResponse{TError}"/>.
/// </summary>
[ApiController]
public class ApiControllerBase : ControllerBase
{
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
    ///     Handle command response and return 204 if success, 400 if error.
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
            return Ok(response.Response);
        }

        return HandleCommandResponse((CommandResponse<TError>)response);
    }

    private IActionResult HandleErrorCommandResponse<TError>(CommandResponse<TError> response)
        where TError : Enumeration
    {
        if (response.IsValidationError)
        {
            return BadRequest(response.ValidationError!.Message);
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return StatusCode(429);
        }

        return BadRequest(response.ErrorCode?.Name ?? response.ErrorMessage);
    }
}
