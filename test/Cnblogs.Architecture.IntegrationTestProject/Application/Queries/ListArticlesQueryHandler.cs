using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;
using Mapster;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class ListArticlesQueryHandler<TDto> : IPageableModelQueryHandler<ListArticlesQuery<TDto>, TDto>
    where TDto : IModel
{
    private static readonly ArticleDto[] Articles = [new() { Id = 1, Title = "作为一个高中生开发者，我的所思所想" }];

    /// <inheritdoc />
    public Task<PagedList<TDto>> Handle(ListArticlesQuery<TDto> request, CancellationToken cancellationToken)
    {
        var dto = Articles.Adapt<List<TDto>>();
        return Task.FromResult(new PagedList<TDto>(dto, request.PagingParams, Articles.Length));
    }
}
