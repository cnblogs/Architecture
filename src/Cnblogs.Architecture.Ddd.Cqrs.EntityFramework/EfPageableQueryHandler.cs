using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.Ddd.Cqrs.EntityFramework;

/// <summary>
///     使用 EF Core 进行分页查询处理。
/// </summary>
/// <typeparam name="TQuery">查询请求。</typeparam>
/// <typeparam name="TEntity">查询使用的实体。</typeparam>
/// <typeparam name="TView">查询最后的结果。</typeparam>
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