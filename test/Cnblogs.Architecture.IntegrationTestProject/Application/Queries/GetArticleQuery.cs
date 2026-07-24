using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

[AgentTool(Description = "Get a single article by its numeric id.")]
public record GetArticleQuery(int Id) : IModelQuery<ArticleDto>;
