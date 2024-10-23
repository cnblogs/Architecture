using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using MongoDB.Driver.Linq;

namespace Cnblogs.Architecture.Ddd.Cqrs.MongoDb;

/// <summary>
///     使用 MongoDb 进行分页查询。
/// </summary>
/// <typeparam name="TQuery">查询参数。</typeparam>
/// <typeparam name="TEntity">实体类型。</typeparam>
/// <typeparam name="TView">视图类型。</typeparam>
public abstract class MongoPageableQueryHandler<TQuery, TEntity, TView>
    : PageableQueryHandlerBase<TQuery, TEntity, TView>
    where TQuery : IPageableQuery<TView>
{
    /// <inheritdoc />
    protected override Task<int> CountAsync(TQuery query, IQueryable<TEntity> queryable)
    {
        return queryable.CountAsync();
    }

    /// <inheritdoc />
    protected override Task<List<TView>> ToListAsync(TQuery query, IQueryable<TView> queryable)
    {
        return queryable.ToListAsync();
    }
}
