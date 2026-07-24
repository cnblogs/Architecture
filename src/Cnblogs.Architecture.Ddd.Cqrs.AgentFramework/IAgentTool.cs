namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Marker interface that opts a Command or Query in to agent-tool exposure. An alternative to <see cref="AgentToolAttribute" />
///     for when you cannot add an attribute to the type but can edit its declaration.
/// </summary>
public interface IAgentTool;
