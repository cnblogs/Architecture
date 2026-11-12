using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     <see cref="CqrsInjector" /> extensions that expose CQRS Commands and Queries as Model Context Protocol (MCP) tools
///     over the Streamable HTTP transport (stateless, the MCP SDK default).
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     Registers an MCP server (Streamable HTTP, stateless — horizontally scalable without session affinity) and the
    ///     runtime tool collection filled by <c>MapMcpTool(...)</c>. After building the app, map the endpoint with
    ///     <c>app.MapMcp("/mcp")</c> (optionally chained with <c>RequireAuthorization()</c>/<c>RequireCors(...)</c>); it coexists
    ///     with <c>MapCommand</c>/<c>MapQuery</c>/<c>MapAgent</c>. Tool calls dispatch in-process via <c>IMediator</c> through the
    ///     same validation/logging/cache/lock/enricher pipeline. MCP capabilities the framework does not wrap (prompts,
    ///     resources, skills, tasks, sampling) can still be added by calling <c>AddMcpServer()</c> again — it is idempotent —
    ///     and chaining the corresponding SDK extension (for example <c>WithSkillsFromSubdirectories(...)</c>).
    /// </summary>
    /// <param name="injector">The <see cref="CqrsInjector" />.</param>
    /// <returns>The <paramref name="injector" />, for chaining.</returns>
    /// <example>
    ///     builder.Services.AddCqrs(Assembly.GetExecutingAssembly())
    ///         .AddMcp();
    ///     // ... after builder.Build():
    ///     app.MapMcpTool("get_article", (int id) => new GetArticleQuery(id), "Get an article by id.");
    ///     app.MapMcp("/mcp").RequireAuthorization();
    /// </example>
    public static CqrsInjector AddMcp(this CqrsInjector injector)
    {
        injector.Services.AddHttpContextAccessor();
        injector.Services.TryAddSingleton<CqrsMcpToolCollection>();
        injector.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<McpServerOptions>, CqrsMcpOptionsSetup>());
        injector.Services.AddMcpServer()
            .WithHttpTransport();
        return injector;
    }
}
