using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.MongoDb;

/// <summary>
///     Handle pageable query using mongodb.
/// </summary>
/// <typeparam name="TQuery">The query parameters.</typeparam>
/// <typeparam name="TEntity">The entity to query.</typeparam>
/// <typeparam name="TModel">The model to map to.</typeparam>
public abstract class MongoPageableModelQueryHandler<TQuery, TEntity, TModel>
    : MongoPageableQueryHandler<TQuery, TEntity, TModel>, IPageableModelQueryHandler<TQuery, TModel>
    where TQuery : IPageableModelQuery<TModel>
    where TModel : IModel
{
}
