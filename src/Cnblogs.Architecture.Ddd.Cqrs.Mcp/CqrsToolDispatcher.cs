using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Dispatches a CQRS request built by a tool in-process via <c>IMediator</c>, opening a scope from the MCP request's
///     service provider so the MediatR pipeline (validation/logging/cache/lock/enricher) and the handler run with the
///     correct lifetimes.
/// </summary>
internal static class CqrsToolDispatcher
{
    /// <summary>
    ///     Sends <paramref name="request" /> through <c>IMediator</c> and marshals the response.
    /// </summary>
    /// <param name="request">The Command/Query built by the tool.</param>
    /// <param name="services">The MCP request's service provider (the request scope in stateless mode).</param>
    /// <param name="cancellationToken">The cancellation token of the MCP request.</param>
    /// <returns>The marshalled, LLM-consumable result.</returns>
    public static async Task<object?> DispatchAsync<TRequest>(TRequest request, IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(request, cancellationToken);
        return CqrsResponseMarshaler.Marshal(response);
    }
}
