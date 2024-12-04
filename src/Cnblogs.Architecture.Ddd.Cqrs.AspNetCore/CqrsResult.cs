using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Send object as json and append X-Cqrs-Version header
/// </summary>
/// <param name="commandResponse">Response body.</param>
/// <param name="options"><see cref="JsonSerializerOptions"/> to use.</param>
public class CqrsResult(object commandResponse, JsonSerializerOptions? options = null) : IResult
{
    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("X-Cqrs-Version", "2");
        return httpContext.Response.WriteAsJsonAsync(commandResponse, options);
    }
}
