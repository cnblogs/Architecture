using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Wraps the CQRS agent tools from <see cref="CqrsToolSet" /> as MCP tools and adds them to
///     <see cref="McpServerOptions.ToolCollection" />. The MCP server invokes each wrapped tool with the request-scoped
///     <c>IServiceProvider</c>, so tool calls open their own scope and dispatch through <c>IMediator</c> as usual.
/// </summary>
internal sealed class CqrsMcpOptionsSetup(CqrsToolSet toolSet, IOptions<CqrsMcpOptions> cqrsMcpOptions)
    : IConfigureOptions<McpServerOptions>
{
    /// <inheritdoc />
    public void Configure(McpServerOptions options)
    {
        var filter = cqrsMcpOptions.Value.RequestFilter;
        var annotateReadOnly = cqrsMcpOptions.Value.AnnotateQueriesAsReadOnly;
        var toolCollection = options.ToolCollection ?? [];
        foreach (var (requestType, function) in toolSet.ByRequestType)
        {
            if (filter is not null && filter(requestType) == false)
            {
                continue;
            }

            toolCollection.TryAdd(McpServerTool.Create(
                function,
                new McpServerToolCreateOptions
                {
                    ReadOnly = annotateReadOnly && CqrsRequestInspector.GetKind(requestType) == RequestKind.Query,
                }));
        }

        if (toolCollection.IsEmpty == false)
        {
            options.ToolCollection = toolCollection;
        }
    }
}
