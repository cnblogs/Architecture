using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Infrastructure;
using Cnblogs.Architecture.IntegrationTestProject.Models;
using Mapster;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class ListArticlesQueryHandler<TDto> : IPageableModelQueryHandler<ListArticlesQuery<TDto>, TDto>
    where TDto : IModel
{
    /// <inheritdoc />
    public Task<PagedList<TDto>> Handle(ListArticlesQuery<TDto> request, CancellationToken cancellationToken)
    {
        var dto = TestData.Articles.Adapt<List<TDto>>();
        return Task.FromResult(new PagedList<TDto>(dto, request.PagingParams, TestData.Articles.Length));
    }
}
