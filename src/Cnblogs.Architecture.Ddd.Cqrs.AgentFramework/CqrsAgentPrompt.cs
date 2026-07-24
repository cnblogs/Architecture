namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     The request body accepted by a CQRS agent endpoint.
/// </summary>
/// <param name="Prompt">The user's natural-language prompt for the agent.</param>
public record CqrsAgentPrompt(string? Prompt);
