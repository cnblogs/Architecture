using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

/// <summary>
///     <see cref="Enumeration" />-derived error type used by the agent-framework test commands.
/// </summary>
#pragma warning disable SA1649
public class AgentTestError(int id, string name) : Enumeration(id, name)
#pragma warning restore SA1649
{
    public static readonly AgentTestError Default = new(1, "DefaultError");
}

/// <summary>Tagged command whose <c>ValidateOnly</c> infra parameter must be hidden.</summary>
[AgentTool(Description = "Create an article.")]
public record AgentCreateCommand(string Title, int Count, bool ValidateOnly = false) : ICommand<AgentTestError>;

/// <summary>Tagged command that explicitly allows <c>ValidateOnly</c> in its schema.</summary>
[AgentTool(AllowValidateOnly = true)]
public record AgentValidateCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>;

/// <summary>Tagged command returning a view, with a nested record parameter.</summary>
public record AgentCreatePayload(string Body, string[] Tags);

[AgentTool]
public record AgentCreateArticleCommand(AgentCreatePayload Payload, bool ValidateOnly = false) : ICommand<string, AgentTestError>;

/// <summary>Tagged single-item query.</summary>
[AgentTool]
public record AgentItemQuery(string? AppId = null, int? Id = null) : IQuery<string>;

/// <summary>Tagged pageable query; <c>PagingParams</c>/<c>OrderByString</c> must be flattened/hidden.</summary>
[AgentTool]
public record AgentPagedQuery(PagingParams? PagingParams, string? OrderByString) : IPageableQuery<string>;

/// <summary>Untagged command (excluded under <see cref="ToolDiscovery.Marked" />).</summary>
public record AgentUntaggedCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>;

/// <summary>Tagged-but-excluded command.</summary>
[AgentTool(Exposure = ToolExposure.Exclude)]
public record AgentExcludedCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>;

/// <summary>Command opted in via the marker interface rather than the attribute.</summary>
public record AgentMarkerCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>, IAgentTool;

/// <summary>Two distinct types with the same simple name, to exercise duplicate-name detection.</summary>
public static class CollisionSetA
{
    [AgentTool]
    public record CollideCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>;
}

/// <summary>Two distinct types with the same simple name, to exercise duplicate-name detection.</summary>
public static class CollisionSetB
{
    [AgentTool]
    public record CollideCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>;
}

/// <summary>
///     Typed agent fixture used by the registration/discovery/factory tests. Restricted to <see cref="AgentCreateCommand" />.
/// </summary>
public class AgentTestAgent : ICqrsAgent
{
    /// <inheritdoc />
    public string Name => "test-agent";

    /// <inheritdoc />
    public string Instructions => "A test agent.";

    /// <inheritdoc />
    public IReadOnlyList<Type> AllowedRequestTypes => [typeof(AgentCreateCommand)];
}

/// <summary>Typed agent fixture with no allow-list, exercising the "all tools" path.</summary>
public class AgentOpenAgent : ICqrsAgent
{
    /// <inheritdoc />
    public string Name => "open-agent";

    /// <inheritdoc />
    public string Instructions => "A test agent.";
}
