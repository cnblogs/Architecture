using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     <see cref="IEndpointRouteBuilder" /> extensions that run a typed CQRS <see cref="CqrsAgentFactory" /> agent behind an HTTP
///     endpoint, coexisting with <c>MapCommand</c>/<c>MapQuery</c>.
/// </summary>
public static class CqrsAgentEndpointExtensions
{
    /// <summary>
    ///     Maps a POST endpoint at <paramref name="route" /> that runs the typed agent <typeparamref name="TAgent" />. The agent
    ///     orchestrates the CQRS Commands/Queries it allows, dispatching each tool call in-process through <c>IMediator</c>.
    ///     Coexists with <c>MapCommand</c>/<c>MapQuery</c>.
    /// </summary>
    /// <typeparam name="TAgent">A concrete <see cref="ICqrsAgent" /> type discovered by <c>AddAgentFramework()</c>.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="route">The route to map the agent endpoint to.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder" /> for further configuration.</returns>
    /// <example>
    ///     app.MapAgent&lt;ArticlesAgent&gt;("/agents/articles");
    /// </example>
    public static IEndpointConventionBuilder MapAgent<TAgent>(this IEndpointRouteBuilder endpoints, string route)
        where TAgent : ICqrsAgent
    {
        return endpoints.MapPost(
            route,
            async (CqrsAgentPrompt prompt, HttpContext httpContext) =>
            {
                if (string.IsNullOrWhiteSpace(prompt.Prompt))
                {
                    return Results.BadRequest("Request body must contain a non-empty 'prompt'.");
                }

                var agent = httpContext.RequestServices.GetRequiredService<CqrsAgentFactory>().GetOrCreate<TAgent>();
                var session = await agent.CreateSessionAsync(httpContext.RequestAborted);
                var response = await agent.RunAsync(prompt.Prompt!, session, null, httpContext.RequestAborted);
                return Results.Ok(new { response = response.Text });
            });
    }
}
