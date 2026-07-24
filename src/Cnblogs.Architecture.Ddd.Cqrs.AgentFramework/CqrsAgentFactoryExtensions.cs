using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     <see cref="IServiceProvider" /> extensions for resolving typed CQRS agents for direct (non-HTTP) invocation.
/// </summary>
public static class CqrsAgentFactoryExtensions
{
    /// <summary>
    ///     Resolves the <see cref="CqrsAgentFactory" /> and returns the cached <see cref="CqrsAgentFactory.GetOrCreate{TAgent}" />
    ///     agent for <typeparamref name="TAgent" />, for direct invocation (e.g. <c>await agent.RunAsync(prompt, session, null, ct)</c>).
    /// </summary>
    /// <param name="services">The application service provider.</param>
    /// <typeparam name="TAgent">A concrete <see cref="ICqrsAgent" /> type discovered by <c>AddAgentFramework()</c>.</typeparam>
    /// <returns>The <see cref="Microsoft.Agents.AI.ChatClientAgent" /> for <typeparamref name="TAgent" />.</returns>
    public static ChatClientAgent GetCqrsAgent<TAgent>(this IServiceProvider services)
        where TAgent : ICqrsAgent
    {
        return services.GetRequiredService<CqrsAgentFactory>().GetOrCreate<TAgent>();
    }
}
