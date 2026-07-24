namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Controls how Command/Query types in the scanned assemblies become agent tools.
/// </summary>
public enum ToolDiscovery
{
    /// <summary>
    ///     Only types explicitly opted in via <see cref="AgentToolAttribute" /> or <see cref="IAgentTool" /> are exposed. This is the default.
    /// </summary>
    Marked = 0,

    /// <summary>
    ///     Every Command/Query in the scanned assemblies is exposed unless tagged <c>[AgentTool(Exposure = ToolExposure.Exclude)]</c>.
    /// </summary>
    AllValidRequests = 1,
}
