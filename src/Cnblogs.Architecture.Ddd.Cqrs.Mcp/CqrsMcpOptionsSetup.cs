using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Adds the tools from <see cref="CqrsMcpToolCollection" /> to <see cref="McpServerOptions.ToolCollection" />. In the
///     default stateless mode the MCP server rebuilds <see cref="McpServerOptions" /> per request, so tools registered
///     during endpoint mapping are picked up automatically.
/// </summary>
internal sealed class CqrsMcpOptionsSetup(CqrsMcpToolCollection toolCollection) : IConfigureOptions<McpServerOptions>
{
    /// <inheritdoc />
    public void Configure(McpServerOptions options)
    {
        var toolCollectionToAdd = options.ToolCollection ?? [];
        foreach (var tool in toolCollection.Tools)
        {
            toolCollectionToAdd.TryAdd(tool);
        }

        if (toolCollectionToAdd.IsEmpty == false)
        {
            options.ToolCollection = toolCollectionToAdd;
        }
    }
}
