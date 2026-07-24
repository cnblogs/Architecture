namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Controls whether a Command or Query is exposed to an AI agent as a tool.
/// </summary>
public enum ToolExposure
{
    /// <summary>
    ///     The type is exposed as a tool (the default).
    /// </summary>
    Include = 0,

    /// <summary>
    ///     The type is never exposed as a tool. Use to opt out under <see cref="ToolDiscovery.AllValidRequests" />.
    /// </summary>
    Exclude = 1,
}
