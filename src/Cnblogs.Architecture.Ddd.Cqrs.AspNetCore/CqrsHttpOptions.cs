using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Configure options for mapping cqrs responses into http responses.
/// </summary>
public class CqrsHttpOptions
{
    /// <summary>
    ///     Configure the http response type for command errors.
    /// </summary>
    public ErrorResponseType CommandErrorResponseType { get; set; } = ErrorResponseType.PlainText;

    /// <summary>
    ///     Custom logic to handle error command response.
    /// </summary>
    public Func<CommandResponse, HttpContext, IResult>? CustomCommandErrorResponseMapper { get; set; }
}
