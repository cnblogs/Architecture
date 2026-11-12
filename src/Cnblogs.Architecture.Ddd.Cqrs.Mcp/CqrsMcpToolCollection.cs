using System.Collections.Concurrent;

using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     The runtime registry of MCP tools registered via <c>MapMcpTool(...)</c>. Tools are added during endpoint mapping
///     (before the first MCP request) and served by the MCP server from every session. Register tools before the
///     application starts handling requests.
/// </summary>
public sealed class CqrsMcpToolCollection
{
    private readonly ConcurrentDictionary<string, McpServerTool> _tools = new(StringComparer.Ordinal);

    /// <summary>
    ///     The registered tools, keyed by tool name.
    /// </summary>
    public IReadOnlyList<McpServerTool> Tools => [.. _tools.Values];

    /// <summary>
    ///     Adds a tool to the collection.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    /// <exception cref="InvalidOperationException">A tool with the same name is already registered.</exception>
    public void Add(McpServerTool tool)
    {
        if (_tools.TryAdd(tool.ProtocolTool.Name, tool) == false)
        {
            throw new InvalidOperationException(
                $"Duplicate MCP tool name '{tool.ProtocolTool.Name}'. Use a different tool or group name.");
        }
    }
}
