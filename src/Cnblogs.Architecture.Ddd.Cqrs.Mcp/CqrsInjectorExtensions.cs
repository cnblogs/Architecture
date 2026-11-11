using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     <see cref="CqrsInjector" /> extensions that expose the CQRS agent tools over the Model Context Protocol (MCP) Streamable HTTP transport.
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     Registers an MCP server (Streamable HTTP, stateless) and exposes the tools built by <c>AddAgentFramework()</c> as MCP tools,
    ///     calling <c>AddAgentFramework()</c> automatically when it has not been called yet. After building the app, map the endpoint with
    ///     <c>MapMcp("/mcp")</c> (optionally chained with <c>RequireAuthorization()</c>/<c>RequireCors(...)</c>); it coexists with
    ///     <c>MapCommand</c>/<c>MapQuery</c>/<c>MapAgent</c>. Tool calls dispatch in-process via <c>IMediator</c> through the same
    ///     validation/logging/cache/lock/enricher pipeline. For advanced transport settings (for example stateful sessions), call
    ///     <c>AddMcpServer().WithHttpTransport(...).WithCqrsTools()</c> manually instead.
    /// </summary>
    /// <param name="injector">The <see cref="CqrsInjector" />.</param>
    /// <param name="configure">Optional configuration of <see cref="CqrsMcpOptions" />.</param>
    /// <returns>The <paramref name="injector" />, for chaining.</returns>
    /// <example>
    ///     builder.Services.AddCqrs(Assembly.GetExecutingAssembly())
    ///         .AddMcp();
    ///     // ... after builder.Build():
    ///     app.MapMcp("/mcp").RequireAuthorization();
    /// </example>
    public static CqrsInjector AddMcp(this CqrsInjector injector, Action<CqrsMcpOptions>? configure = null)
    {
        if (injector.Services.All(d => d.ServiceType != typeof(CqrsAgentOptions)))
        {
            injector.AddAgentFramework();
        }

        injector.Services.AddMcpServer()
            .WithHttpTransport()
            .WithCqrsTools(configure);

        return injector;
    }
}
