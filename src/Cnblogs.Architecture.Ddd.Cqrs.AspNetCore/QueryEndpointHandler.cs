using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     The query executor, auto send query to <see cref="IMediator"/>.
/// </summary>
public class QueryEndpointHandler(IMediator mediator, IOptions<CqrsHttpOptions> cqrsHttpOptions) : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var query = await next(context);
        if (query is null)
        {
            return query;
        }

        if (query is not IBaseRequest)
        {
            return query;
        }

        var response = await mediator.Send(query);
        Console.WriteLine("QueryEndpointHandler");
        return response == null
            ? Results.NotFound()
            : Results.Json(response);
    }
}
