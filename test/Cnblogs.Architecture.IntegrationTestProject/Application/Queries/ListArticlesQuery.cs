using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public record ListArticlesQuery<TDto>(PagingParams? PagingParams, string? OrderByString)
    : IPageableModelQuery<TDto>
    where TDto : IModel;
