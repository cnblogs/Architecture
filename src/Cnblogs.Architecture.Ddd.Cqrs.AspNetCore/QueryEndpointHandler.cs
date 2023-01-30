using MediatR;

using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     查询执行器，自动将返回内容提交给 mediator 并返回结果。
/// </summary>
public class QueryEndpointHandler : IEndpointFilter
{
    private readonly IMediator _mediator;

    /// <summary>
    ///     创建一个查询执行器。
    /// </summary>
    /// <param name="mediator"></param>
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

        var response = await _mediator.Send(query);
        return response;
    }
}