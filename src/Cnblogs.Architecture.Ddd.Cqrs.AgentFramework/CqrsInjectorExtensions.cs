using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     <see cref="CqrsInjector" /> extensions that register Microsoft.Extensions.AI agent tools for opted-in CQRS Commands and Queries.
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     Scans the assemblies passed to <see cref="CqrsInjector" /> for opted-in Commands/Queries, registers one <see cref="AIFunction" /> per type
    ///     (aggregated in a <see cref="CqrsToolSet" /> singleton), discovers concrete <see cref="ICqrsAgent" /> types (registered automatically), and
    ///     registers the <see cref="CqrsAgentFactory" /> that hosts them as Microsoft Agent Framework agents. Tools dispatch in-process
    ///     via <c>IMediator</c>. Chain this off <c>AddCqrs(...)</c>; then map agents over HTTP with <c>MapAgent&lt;TAgent&gt;()</c> or resolve them
    ///     directly with <c>GetCqrsAgent&lt;TAgent&gt;()</c>.
    /// </summary>
    /// <param name="injector">The <see cref="CqrsInjector" />.</param>
    /// <param name="configure">Optional configuration of <see cref="CqrsAgentOptions" />.</param>
    /// <returns>The <paramref name="injector" />, for chaining.</returns>
    /// <example>
    ///     builder.Services.AddCqrs(Assembly.GetExecutingAssembly())
    ///         .AddAgentFramework(o =&gt; o.Discovery = ToolDiscovery.Marked);
    /// </example>
    public static CqrsInjector AddAgentFramework(this CqrsInjector injector, Action<CqrsAgentOptions>? configure = null)
    {
        var options = new CqrsAgentOptions();
        configure?.Invoke(options);

        injector.Services.AddSingleton(options);
        injector.Services.AddSingleton<CqrsToolSet>(_ =>
        {
            var xmlDocumentation = new XmlDocumentationProvider();
            var descriptors = CqrsToolScanner.Scan(injector.Assemblies, options, xmlDocumentation);
            var byRequestType = descriptors.ToDictionary(descriptor => descriptor.RequestType, descriptor => (AIFunction)new CqrsToolFunction(descriptor));
            return new CqrsToolSet(byRequestType.Values.ToList(), byRequestType);
        });

        var agentTypes = injector.Assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                        && typeof(ICqrsAgent).IsAssignableFrom(t))
            .ToList();
        foreach (var agentType in agentTypes)
        {
            injector.Services.AddSingleton(agentType);
        }

        injector.Services.AddSingleton(new CqrsAgentRegistry(agentTypes));
        injector.Services.AddSingleton<CqrsAgentFactory>();

        return injector;
    }
}
