using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Send command response as json and report current cqrs version.
/// </summary>
/// <param name="value"></param>
public class CqrsObjectResult(object? value) : ObjectResult(value)
{
    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.Headers.AppendCurrentCqrsVersion();
        return base.ExecuteResultAsync(context);
    }
}
