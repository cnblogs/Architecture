using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Extension methods to configure behaviors of mapping command/query response to http response.
/// </summary>
public static class CqrsHttpOptionsInjector
{
    /// <summary>
    ///     Use <see cref="ProblemDetails"/> to represent command response.
    /// </summary>
    /// <param name="injector">The <see cref="CqrsInjector"/>.</param>
    /// <returns></returns>
    public static CqrsInjector UseProblemDetails(this CqrsInjector injector)
    {
        injector.Services.AddProblemDetails();
        injector.Services.Configure<CqrsHttpOptions>(
            c => c.CommandErrorResponseType = ErrorResponseType.ProblemDetails);
        return injector;
    }

    /// <summary>
    ///     Use custom mapper to convert command response into HTTP response.
    /// </summary>
    /// <param name="injector">The <see cref="CqrsInjector"/>.</param>
    /// <param name="mapper">The custom map function.</param>
    /// <returns></returns>
    public static CqrsInjector UseCustomCommandErrorResponseMapper(
        this CqrsInjector injector,
        Func<CommandResponse, HttpContext, IResult> mapper)
    {
        injector.Services.Configure<CqrsHttpOptions>(
            c =>
            {
                c.CommandErrorResponseType = ErrorResponseType.Custom;
                c.CustomCommandErrorResponseMapper = mapper;
            });
        return injector;
    }
}
