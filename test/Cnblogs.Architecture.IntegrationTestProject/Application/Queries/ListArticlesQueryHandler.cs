using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class ListArticlesQueryHandler : IPageableQueryHandler<ListArticlesQuery, ArticleDto>
{
    private static readonly ArticleDto[] Articles =
    [
        new ArticleDto
        {
            Id = 1,
            Title = "作为一个高中生开发者，我的所思所想"
        }
    ];

    /// <inheritdoc />
    public Task<PagedList<ArticleDto>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PagedList<ArticleDto>(Articles, request.PagingParams, Articles.Length));
    }
}
