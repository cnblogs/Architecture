using MediatR;

using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     The query executor, auto send query to <see cref="IMediator"/>.
/// </summary>
public class QueryEndpointHandler : IEndpointFilter
{
    private readonly IMediator _mediator;

    /// <summary>
    ///     Create a <see cref="QueryEndpointHandler"/>.
    /// </summary>
    /// <param name="mediator">The mediator to use.</param>
    public QueryEndpointHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

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

        var response = await _mediator.Send(query);
        return response == null ? Results.NotFound() : Results.Ok(response);
    }
}
