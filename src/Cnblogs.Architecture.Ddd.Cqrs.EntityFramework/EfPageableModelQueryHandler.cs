using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.EntityFramework;

/// <summary>
///     Handle pageable query with EF Core.
/// </summary>
/// <typeparam name="TQuery">The query parameters type</typeparam>
/// <typeparam name="TEntity">The entity to query.</typeparam>
/// <typeparam name="TModel">The type to map to.</typeparam>
public abstract class EfPageableModelQueryHandler<TQuery, TEntity, TModel>
    : EfPageableQueryHandler<TQuery, TEntity, TModel>,
        IPageableModelQueryHandler<TQuery, TModel>
    where TModel : IModel
    where TQuery : IPageableModelQuery<TModel>
{
}
