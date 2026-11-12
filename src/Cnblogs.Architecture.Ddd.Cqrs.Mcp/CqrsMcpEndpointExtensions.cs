using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     <see cref="IEndpointRouteBuilder" /> extensions that register CQRS Commands and Queries as MCP tools, in the same
///     delegate-first style as <c>MapCommand</c>/<c>MapQuery</c>. Requires <c>AddCqrs(...).AddMcp()</c>; serve the tools with
///     <c>MapMcp("/mcp")</c>.
/// </summary>
public static class CqrsMcpEndpointExtensions
{
    /// <summary>
    ///     Exposes <typeparamref name="TRequest" /> as a record-bound MCP tool: the schema is flattened from the request
    ///     record's constructor parameters, and CQRS infrastructure parameters (<c>ValidateOnly</c>, <c>OrderByString</c>,
    ///     <c>PagingParams</c> — paging is flattened to <c>pageIndex</c>/<c>pageSize</c> with the page size capped) are hidden and
    ///     bound server-side. Tool calls dispatch in-process via <c>IMediator</c>. Parameter descriptions and the default tool
    ///     description come from the record's XML doc (<c>&lt;param&gt;</c>/<c>&lt;summary&gt;</c>) when available. When the request
    ///     needs context values (like a user id) or parameter shaping, use a delegate overload instead.
    /// </summary>
    /// <typeparam name="TRequest">The Command/Query type.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="name">The tool name, shown to the model.</param>
    /// <param name="description">The tool description, shown to the model.</param>
    /// <param name="configure">Optional fine-tuning of the tool (title, destructive/idempotent hints, metadata).</param>
    /// <exception cref="InvalidOperationException">A tool with the same name is already registered.</exception>
    /// <example>
    ///     app.MapMcpTool&lt;GetArticleQuery&gt;("get_article", "Get an article by id.");
    /// </example>
    public static void MapMcpTool<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string name,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddRecordTool<TRequest>(GetTools(endpoints), name, description, configure);

    /// <summary>
    ///     Exposes <paramref name="factory" /> as an MCP tool. The delegate builds the Command/Query from model-supplied
    ///     arguments and dispatches in-process via <c>IMediator</c>; the response is marshalled for the model. Parameters of
    ///     type <see cref="System.Security.Claims.ClaimsPrincipal" /> (the calling user), services registered in DI,
    ///     <c>IProgress&lt;ProgressNotificationValue&gt;</c> and <c>CancellationToken</c> are injected by the framework and hidden from
    ///     the schema — use them to pass <c>HttpContext</c>-derived values (user ids, tenants) into the request so the model
    ///     cannot forge them.
    /// </summary>
    /// <remarks>
    ///     Parameter descriptions, default values and required-ness are derived from the delegate's signature. Apply
    ///     <see cref="System.ComponentModel.DescriptionAttribute" /> to lambda parameters (supported since C# 10, the same
    ///     mechanism minimal APIs use for <c>[FromRoute]</c>/<c>[FromBody]</c>) to describe each argument to the model:
    ///     <code>
    ///         app.MapMcpTool(
    ///             "create_article",
    ///             ([Description("The title of the article.")] string title,
    ///              [Description("Whether to only run validation.")] bool validateOnly = false,
    ///              ClaimsPrincipal user)
    ///                 => new CreateArticleCommand(user.GetUserId(), title, validateOnly),
    ///             "Create an article for the current user.");
    ///     </code>
    ///     HTTP-specific binding attributes such as <c>[FromBody]</c> have no effect in the MCP context.
    /// </remarks>
    /// <typeparam name="TRequest">The Command/Query type returned by the factory.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="name">The tool name, shown to the model.</param>
    /// <param name="factory">Builds the request from tool arguments.</param>
    /// <param name="description">The tool description, shown to the model.</param>
    /// <param name="configure">Optional fine-tuning of the tool (title, destructive/idempotent hints, metadata).</param>
    /// <exception cref="InvalidOperationException">A tool with the same name is already registered.</exception>
    public static void MapMcpTool<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string name,
        Func<TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddDelegateTool<TRequest>(endpoints, name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(IEndpointRouteBuilder, string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public static void MapMcpTool<TRequest, T1>(
        this IEndpointRouteBuilder endpoints,
        string name,
        Func<T1, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddDelegateTool<TRequest>(endpoints, name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(IEndpointRouteBuilder, string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public static void MapMcpTool<TRequest, T1, T2>(
        this IEndpointRouteBuilder endpoints,
        string name,
        Func<T1, T2, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddDelegateTool<TRequest>(endpoints, name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(IEndpointRouteBuilder, string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public static void MapMcpTool<TRequest, T1, T2, T3>(
        this IEndpointRouteBuilder endpoints,
        string name,
        Func<T1, T2, T3, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddDelegateTool<TRequest>(endpoints, name, description, configure, factory);

    /// <inheritdoc cref="MapMcpTool{TRequest}(IEndpointRouteBuilder, string, Func{TRequest}, string?, Action{McpServerToolCreateOptions}?)" />
    public static void MapMcpTool<TRequest, T1, T2, T3, T4>(
        this IEndpointRouteBuilder endpoints,
        string name,
        Func<T1, T2, T3, T4, TRequest> factory,
        string? description = null,
        Action<McpServerToolCreateOptions>? configure = null)
        => AddDelegateTool<TRequest>(endpoints, name, description, configure, factory);

    /// <summary>
    ///     Creates a named group of MCP tools; tool names inside the group are prefixed with <c>"{name}_"</c> (for example
    ///     <c>articles_get</c>), following the MCP convention for multi-domain servers.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="name">The group name, used as the tool-name prefix.</param>
    /// <returns>The group, on which tools are registered with <c>MapMcpTool(...)</c>.</returns>
    /// <example>
    ///     var articles = app.MapMcpToolGroup("articles");
    ///     articles.MapMcpTool("get", (int id) => new GetArticleQuery(id), "Get an article by id.");
    /// </example>
    public static CqrsMcpToolGroup MapMcpToolGroup(this IEndpointRouteBuilder endpoints, string name)
        => new(name, GetTools(endpoints), endpoints.ServiceProvider);

    internal static void AddRecordTool<TRequest>(
        CqrsMcpToolCollection tools,
        string name,
        string? description,
        Action<McpServerToolCreateOptions>? configure)
    {
        // Fall back to the request record's XML doc summary so one comment serves both HTTP docs and the tool schema.
        var effectiveDescription = description ?? CqrsToolRequestBinder<TRequest>.TypeSummary;
        var options = BuildCreateOptions<TRequest>(name, effectiveDescription, configure);
        tools.Add(McpServerTool.Create(
            new CqrsRecordToolFunction<TRequest>(name, effectiveDescription ?? string.Empty),
            options));
    }

    internal static void AddDelegateTool<TRequest>(
        IEndpointRouteBuilder endpoints,
        string name,
        string? description,
        Action<McpServerToolCreateOptions>? configure,
        Delegate factory)
    {
        var options = BuildCreateOptions<TRequest>(name, description, configure);
        var function = new CqrsDelegateToolFunction<TRequest>(factory, name, description ?? string.Empty, endpoints.ServiceProvider);
        GetTools(endpoints).Add(McpServerTool.Create(function, options));
    }

    private static McpServerToolCreateOptions BuildCreateOptions<TRequest>(
        string name,
        string? description,
        Action<McpServerToolCreateOptions>? configure)
    {
        var options = new McpServerToolCreateOptions
        {
            Name = name,
            Description = description,
            ReadOnly = CqrsRequestInspector.IsQuery(typeof(TRequest)),
        };
        configure?.Invoke(options);
        return options;
    }

    private static CqrsMcpToolCollection GetTools(IEndpointRouteBuilder endpoints)
        => endpoints.ServiceProvider.GetRequiredService<CqrsMcpToolCollection>();
}
