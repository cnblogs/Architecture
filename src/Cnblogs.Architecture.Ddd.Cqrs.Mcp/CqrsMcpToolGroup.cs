using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     A named group of MCP tools, created via <c>MapMcpToolGroup(...)</c>. Tool names inside the group are prefixed with
/// <c>"{group name}_"</c> (for example <c>articles_get</c>), following the MCP convention for multi-domain servers. Use
/// groups to keep the tool list organized when many domains are exposed from one server; use separate
/// <c>MapMcp()</c> endpoints (with the MCP SDK's per-session tool filtering) when different clients must see different
/// tool sets.
/// </summary>
public sealed class CqrsMcpToolGroup
{
    internal CqrsMcpToolGroup(string prefix, CqrsMcpToolCollection tools, IServiceProvider applicationServices)
    {
        Prefix = prefix;
        Tools = tools;
        ApplicationServices = applicationServices;
    }

    private string Prefix { get; }

    private CqrsMcpToolCollection Tools { get; }

    private IServiceProvider ApplicationServices { get; }

    /// <summary>
    ///     Exposes the record-bound tool <typeparamref name="TRequest" /> under the group, named
    ///     <c>"{Prefix}_{name}"</c>. The tool's schema is flattened from the request record's constructor parameters;
    ///     CQRS infrastructure parameters are hidden. See the record overload of <c>MapMcpTool</c> for details.
    /// </summary>
    /// <typeparam name="TRequest">The Command/Query type.</typeparam>
    /// <param name="name">The tool name inside the group.</param>
    /// <param name="description">The tool description shown to the model.</param>
    /// <param name="configure">Optional fine-tuning of the tool (title, hints, metadata).</param>
    public void MapMcpTool<TRequest>(string name, string? description = null, Action<McpServerToolCreateOptions>? configure = null)
        => CqrsMcpEndpointExtensions.AddRecordTool<TRequest>(Tools, GetFullName(name), description, configure);

    /// <summary>
    ///     Exposes <paramref name="factory" /> as a tool under the group, named <c>"{Prefix}_{name}"</c>. The delegate
    ///     builds the Command/Query from model-supplied arguments; <see cref="System.Security.Claims.ClaimsPrincipal" />,
    ///     registered services and <c>CancellationToken</c> parameters are injected by the framework and hidden from the
    ///     schema. See <c>MapMcpTool</c> for details.
    /// </summary>
    /// <typeparam name="TRequest">The Command/Query type returned by the factory.</typeparam>
    /// <param name="name">The tool name inside the group.</param>
    /// <param name="factory">Builds the request from tool arguments.</param>
    /// <param name="description">The tool description shown to the model.</param>
    /// <param name="configure">Optional fine-tuning of the tool (title, hints, metadata).</param>
    public void MapMcpTool<TRequest>(
        string name,
        Func<TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddGroupTool<TRequest>(name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public void MapMcpTool<TRequest, T1>(
        string name,
        Func<T1, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddGroupTool<TRequest>(name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public void MapMcpTool<TRequest, T1, T2>(
        string name,
        Func<T1, T2, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddGroupTool<TRequest>(name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public void MapMcpTool<TRequest, T1, T2, T3>(
        string name,
        Func<T1, T2, T3, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddGroupTool<TRequest>(name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public void MapMcpTool<TRequest, T1, T2, T3, T4>(
        string name,
        Func<T1, T2, T3, T4, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddGroupTool<TRequest>(name, description, configure, factory);

    private void AddGroupTool<TRequest>(
        string name,
        string? description,
        Action<McpServerToolCreateOptions>? configure,
        Delegate factory)
    {
        var fullName = GetFullName(name);
        var options = new McpServerToolCreateOptions
        {
            Name = fullName,
            Description = description,
            ReadOnly = CqrsRequestInspector.IsQuery(typeof(TRequest)),
        };
        configure?.Invoke(options);
        var function = new CqrsDelegateToolFunction<TRequest>(factory, fullName, description ?? string.Empty, ApplicationServices);
        Tools.Add(McpServerTool.Create(function, options));
    }

    private string GetFullName(string name) => $"{Prefix}_{name}";
}
