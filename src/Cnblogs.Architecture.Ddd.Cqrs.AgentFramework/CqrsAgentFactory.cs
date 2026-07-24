using System.Collections.Concurrent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Builds and caches a Microsoft Agent Framework <see cref="ChatClientAgent" /> per typed <see cref="ICqrsAgent" />. Each agent's
///     tools are the CQRS Commands/Queries it allows (see <see cref="ICqrsAgent.AllowedRequestTypes" />), dispatched in-process through
///     <c>IMediator</c>. Resolve one with <see cref="CqrsAgentFactoryExtensions.GetCqrsAgent{TAgent}" /> for direct invocation, or map it
///     over HTTP with <see cref="CqrsAgentEndpointExtensions.MapAgent{TAgent}" />. Registered automatically by
///     <see cref="CqrsInjectorExtensions.AddAgentFramework" />.
/// </summary>
public sealed class CqrsAgentFactory
{
    private readonly CqrsAgentRegistry _registry;
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<Type, ChatClientAgent> _agents = new();

    /// <summary>Creates the factory.</summary>
    /// <param name="services">The application (root) service provider, used to resolve the registry, chat client, and tool scopes.</param>
    public CqrsAgentFactory(IServiceProvider services)
    {
        _services = services;
        _registry = services.GetRequiredService<CqrsAgentRegistry>();
    }

    /// <summary>
    ///     Resolves the <see cref="ChatClientAgent" /> for <typeparamref name="TAgent" />, building and caching it on first use from the
    ///     agent's <see cref="ICqrsAgent.Name" />, <see cref="ICqrsAgent.Instructions" />, <see cref="ICqrsAgent.ChatClientServiceKey" />,
    ///     and its allowed tools.
    /// </summary>
    /// <typeparam name="TAgent">A concrete <see cref="ICqrsAgent" /> type discovered by <c>AddAgentFramework()</c>.</typeparam>
    /// <returns>The cached <see cref="ChatClientAgent" /> for <typeparamref name="TAgent" />.</returns>
    public ChatClientAgent GetOrCreate<TAgent>()
        where TAgent : ICqrsAgent
    {
        return _agents.GetOrAdd(typeof(TAgent), Build);
    }

    private ChatClientAgent Build(Type agentType)
    {
        var agent = _registry.Resolve(_services, agentType);
        var tools = _registry.GetTools(_services, agentType);
        var chatClient = agent.ChatClientServiceKey is null
            ? _services.GetRequiredService<IChatClient>()
            : _services.GetRequiredKeyedService<IChatClient>(agent.ChatClientServiceKey);

        return new ChatClientAgent(
            chatClient,
            instructions: agent.Instructions,
            name: agent.Name,
            description: agent.Name,
            tools: new List<AITool>(tools),
            loggerFactory: _services.GetService<ILoggerFactory>(),
            services: _services);
    }
}
