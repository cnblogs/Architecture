using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class GetArticleQueryHandler : IModelQueryHandler<GetArticleQuery, ArticleDto>
{
    /// <inheritdoc />
    public async Task<ArticleDto?> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        return new ArticleDto { Id = request.Id, Title = $"Id 为：{request.Id} 的博文标题" };
    }
}
