using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Resolves registered <see cref="ICqrsAgent" /> instances and the CQRS tools each one is allowed to call (filtered by
///     <see cref="ICqrsAgent.AllowedRequestTypes" />). Built once during <see cref="CqrsInjectorExtensions.AddAgentFramework" />.
/// </summary>
internal sealed class CqrsAgentRegistry
{
    /// <summary>Creates a registry over the discovered agent types.</summary>
    /// <param name="agentTypes">The concrete <see cref="ICqrsAgent" /> types discovered by scanning.</param>
    public CqrsAgentRegistry(IReadOnlyList<Type> agentTypes)
    {
        AgentTypes = agentTypes;
    }

    /// <summary>The concrete <see cref="ICqrsAgent" /> types discovered by scanning.</summary>
    public IReadOnlyList<Type> AgentTypes { get; }

    /// <summary>Resolves the agent instance of <paramref name="agentType" /> from the container.</summary>
    /// <param name="services">The application service provider.</param>
    /// <param name="agentType">A concrete <see cref="ICqrsAgent" /> type present in <see cref="AgentTypes" />.</param>
    /// <returns>The resolved agent instance.</returns>
    public ICqrsAgent Resolve(IServiceProvider services, Type agentType)
    {
        return (ICqrsAgent)services.GetRequiredService(agentType);
    }

    /// <summary>
    ///     The CQRS tools <paramref name="agentType" /> may call: every opted-in tool when its
    ///     <see cref="ICqrsAgent.AllowedRequestTypes" /> is <see langword="null" />/empty, otherwise only the listed subset.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    /// <param name="agentType">A concrete <see cref="ICqrsAgent" /> type present in <see cref="AgentTypes" />.</param>
    /// <returns>The filtered tool list for the agent.</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider services, Type agentType)
    {
        var toolSet = services.GetRequiredService<CqrsToolSet>();
        var agent = Resolve(services, agentType);
        return FilterTools(toolSet.ByRequestType, toolSet.Tools, agent.AllowedRequestTypes, agent.Name);
    }

    /// <summary>
    ///     Pure filter: returns <paramref name="allTools" /> when <paramref name="allowed" /> is <see langword="null" />/empty,
    ///     otherwise only the tools whose request type is listed. Throws <see cref="InvalidOperationException" /> if a listed type
    ///     is not an exposed tool.
    /// </summary>
    /// <param name="byRequestType">The map of request type to tool, from <see cref="CqrsToolSet.ByRequestType" />.</param>
    /// <param name="allTools">Every opted-in tool, from <see cref="CqrsToolSet.Tools" />.</param>
    /// <param name="allowed">The agent's allowed request types, or <see langword="null" />/empty for all.</param>
    /// <param name="agentName">The agent name, used in error messages.</param>
    /// <returns>The filtered tool list.</returns>
    /// <exception cref="InvalidOperationException">A type in <paramref name="allowed" /> is not an exposed tool.</exception>
    public static IReadOnlyList<AIFunction> FilterTools(
        IReadOnlyDictionary<Type, AIFunction> byRequestType,
        IReadOnlyList<AIFunction> allTools,
        IReadOnlyList<Type>? allowed,
        string agentName)
    {
        if (allowed is null || allowed.Count == 0)
        {
            return allTools;
        }

        var tools = new List<AIFunction>(allowed.Count);
        foreach (var requestType in allowed)
        {
            if (byRequestType.TryGetValue(requestType, out var tool))
            {
                tools.Add(tool);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Agent '{agentName}' allows request type '{requestType.FullName}', but it is not exposed as a tool. "
                    + "Tag it [AgentTool] (or implement IAgentTool), or set CqrsAgentOptions.Discovery = AllValidRequests.");
            }
        }

        return tools;
    }
}
