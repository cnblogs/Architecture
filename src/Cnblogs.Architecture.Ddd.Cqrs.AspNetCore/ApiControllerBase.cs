using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
/// Controller 基类，提供自动处理 <see cref="CommandResponse{TError}"/> 的方法。
/// </summary>
[ApiController]
public class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// 处理 CommandResponse 并返回对应的状态码，成功-204，错误-400。
    /// </summary>
    /// <param name="response">任务结果。</param>
    /// <typeparam name="TError">错误类型。</typeparam>
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
    /// 自动处理命令返回的结果，成功-200，失败-400。
    /// </summary>
    /// <param name="response">命令执行结果。</param>
    /// <typeparam name="TResponse">返回类型。</typeparam>
    /// <typeparam name="TError">错误类型。</typeparam>
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

        if (response.IsConcurrentError && response.LockAcquired == false)
        {
            return StatusCode(429);
        }

        return BadRequest(response.ErrorCode?.Name ?? response.ErrorMessage);
    }
}