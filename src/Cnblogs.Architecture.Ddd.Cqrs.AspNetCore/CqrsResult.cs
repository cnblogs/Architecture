﻿using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Send object as json and append X-Cqrs-Version header
/// </summary>
/// <param name="commandResponse"></param>
public class CqrsResult(object commandResponse) : IResult
{
    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("X-Cqrs-Version", "2");
        return httpContext.Response.WriteAsJsonAsync(commandResponse);
    }
}
