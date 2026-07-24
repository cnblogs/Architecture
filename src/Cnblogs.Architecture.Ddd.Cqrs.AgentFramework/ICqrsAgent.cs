using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     A typed AI agent backed by the CQRS tools built by <see cref="CqrsInjectorExtensions.AddAgentFramework" />.
///     Implementations are discovered by scanning the assemblies passed to <see cref="CqrsInjector" /> and registered
///     automatically — no manual registration is required.
/// </summary>
public interface ICqrsAgent
{
    /// <summary>The agent name (also used as its identity and keyed-service key).</summary>
    string Name { get; }

    /// <summary>The system instructions guiding the agent's behavior.</summary>
    string Instructions { get; }

    /// <summary>
    ///     The CQRS request types this agent may call, or <see langword="null" /> / empty to allow every opted-in tool.
    ///     Each type must be exposed as a tool (tagged <c>[AgentTool]</c>, or <c>IAgentTool</c>, or covered by
    ///     <see cref="CqrsAgentOptions.Discovery" />); an entry that is not an exposed tool throws at resolution time.
    /// </summary>
    IReadOnlyList<Type>? AllowedRequestTypes => null;

    /// <summary>
    ///     The keyed-service key of the <c>IChatClient</c> this agent uses. When <see langword="null" />, the default
    ///     <c>IChatClient</c> is resolved.
    /// </summary>
    string? ChatClientServiceKey => null;
}
