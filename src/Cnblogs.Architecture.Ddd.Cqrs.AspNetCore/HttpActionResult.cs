using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Used because the same class in AspNetCore framework is internal.
/// </summary>
internal sealed class HttpActionResult : ActionResult
{
    /// <summary>
    /// Gets the instance of the current <see cref="IResult"/>.
    /// </summary>
    public IResult Result { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpActionResult"/> class with the
    /// <see cref="IResult"/> provided.
    /// </summary>
    /// <param name="result">The <see cref="IResult"/> instance to be used during the <see cref="ExecuteResultAsync"/> invocation.</param>
    public HttpActionResult(IResult result)
    {
        Result = result;
    }

    /// <inheritdoc/>
    public override Task ExecuteResultAsync(ActionContext context) => Result.ExecuteAsync(context.HttpContext);
}
