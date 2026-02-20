using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.Ddd.Cqrs.EntityFramework;

/// <summary>
///     Handle pageable query with EF Core.
/// </summary>
/// <typeparam name="TQuery">The query parameters type</typeparam>
/// <typeparam name="TEntity">The entity to query.</typeparam>
/// <typeparam name="TView">The type to map to.</typeparam>
public abstract class EfPageableQueryHandler<TQuery, TEntity, TView>
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
