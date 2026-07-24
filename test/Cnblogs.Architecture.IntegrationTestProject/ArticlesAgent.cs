using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
using Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

namespace Cnblogs.Architecture.IntegrationTestProject;

/// <summary>
///     Sample typed agent, auto-scanned by <c>AddAgentFramework()</c>. It is restricted to creating and looking up articles.
/// </summary>
public class ArticlesAgent : ICqrsAgent
{
    /// <inheritdoc />
    public string Name => "articles-agent";

    /// <inheritdoc />
    public string Instructions => "You manage articles. Use the provided tools to create and look up articles.";

    /// <inheritdoc />
    public IReadOnlyList<Type> AllowedRequestTypes => [typeof(CreateArticleCommand), typeof(GetArticleQuery)];
}
