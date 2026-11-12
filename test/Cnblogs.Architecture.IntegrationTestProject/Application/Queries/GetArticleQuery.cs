using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

/// <summary>
///     Get a single article by its numeric id.
/// </summary>
/// <param name="Id">The id of the article to get.</param>
[AgentTool(Description = "Get a single article by its numeric id.")]
public record GetArticleQuery(int Id) : IModelQuery<ArticleDto>;
