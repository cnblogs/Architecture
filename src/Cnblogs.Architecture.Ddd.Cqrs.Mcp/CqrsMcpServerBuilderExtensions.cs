using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.Ddd.Cqrs.Mcp;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     <see cref="IMcpServerBuilder" /> extensions that expose the CQRS agent tools from <c>AddAgentFramework()</c> as MCP tools.
/// </summary>
public static class CqrsMcpServerBuilderExtensions
{
    /// <summary>
    ///     Exposes the tools built by <c>AddCqrs(...).AddAgentFramework()</c> as MCP tools, honoring
    ///     <see cref="CqrsMcpOptions.RequestFilter" /> and <see cref="CqrsMcpOptions.AnnotateQueriesAsReadOnly" />. Use this instead of
    ///     <c>AddCqrs(...).AddMcp()</c> when you need to configure the MCP server or its HTTP transport yourself.
    /// </summary>
    /// <param name="builder">The <see cref="IMcpServerBuilder" />.</param>
    /// <param name="configure">Optional configuration of <see cref="CqrsMcpOptions" />.</param>
    /// <returns>The <paramref name="builder" />, for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <c>AddAgentFramework()</c> has not been called on the same service collection, so there is no <see cref="CqrsToolSet" /> to expose.
    /// </exception>
    /// <example>
    ///     builder.Services.AddCqrs(Assembly.GetExecutingAssembly())
    ///         .AddAgentFramework();
    ///     builder.Services.AddMcpServer()
    ///         .WithHttpTransport(o => o.SessionMode = HttpServerSessionMode.Stateful)
    ///         .WithCqrsTools();
    /// </example>
    public static IMcpServerBuilder WithCqrsTools(this IMcpServerBuilder builder, Action<CqrsMcpOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(d => d.ServiceType == typeof(CqrsToolSet)) == false)
        {
            throw new InvalidOperationException(
                "You must call AddCqrs(...).AddAgentFramework() before WithCqrsTools(). Unable to find the CqrsToolSet registration.");
        }

        if (configure is not null)
        {
            builder.Services.Configure(configure);
        }

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<McpServerOptions>, CqrsMcpOptionsSetup>());

        return builder;
    }
}
