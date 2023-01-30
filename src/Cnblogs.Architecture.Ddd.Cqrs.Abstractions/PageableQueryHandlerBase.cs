using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Mapster;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     用于实现 <see cref="IPageableQueryHandler{TQuery,TView}" /> 的基类。
/// </summary>
/// <typeparam name="TQuery">查询类型。</typeparam>
/// <typeparam name="TEntity">实体类型。</typeparam>
/// <typeparam name="TView">返回类型。</typeparam>
public abstract class PageableQueryHandlerBase<TQuery, TEntity, TView> : IPageableQueryHandler<TQuery, TView>
    where TQuery : IPageableQuery<TView>
{
    /// <inheritdoc />
    public async Task<PagedList<TView>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var queryable = await FilterAsync(request);
        var hasOrderBy = OrderBySegmentConfig.TryParseOrderBySegments<TEntity>(
            request.OrderByString,
            out var orderBySegments);

        var ordered = hasOrderBy && orderBySegments is { Count: > 0 }
            ? queryable.OrderBy(orderBySegments)
            : DefaultOrderBy(request, queryable);

        var totalCount = 0;
        if (request.PagingParams != null)
        {
            totalCount = await CountAsync(request, queryable);
            if (request.PagingParams.PageSize == 0 || totalCount == 0)
            {
                // need count only or no available item, short circuit here.
                return new PagedList<TView>(Array.Empty<TView>(), request.PagingParams, totalCount);
            }

            ordered = ordered.Paging(request.PagingParams);
        }

        var items = await ToListAsync(request, ProjectToView(request, ordered));
        return request.PagingParams == null
            ? new PagedList<TView>(items)
            : new PagedList<TView>(items, request.PagingParams, totalCount);
    }

    /// <summary>
    ///     获取总数的查询。
    /// </summary>
    /// <param name="query">查询条件。</param>
    /// <param name="queryable">过滤好的 <see cref="IQueryable{T}" />。</param>
    /// <returns>总数。</returns>
    protected abstract Task<int> CountAsync(TQuery query, IQueryable<TEntity> queryable);

    /// <summary>
    ///     默认的排序条件，如果没有指定 <see cref="OrderBySegment" />，将会使用这一语句。
    /// </summary>
    /// <param name="query">查询条件。</param>
    /// <param name="queryable"><see cref="Filter" />返回的<see cref="IQueryable{TEntity}" />。</param>
    /// <returns>排序好的 <see cref="IQueryable{T}" />。</returns>
    protected abstract IQueryable<TEntity> DefaultOrderBy(TQuery query, IQueryable<TEntity> queryable);

    /// <summary>
    ///     获取并过滤，返回 <see cref="IQueryable{T}" />
    /// </summary>
    /// <param name="query">输入的查询条件。</param>
    /// <returns>过滤后的 <see cref="IQueryable{T}" />。</returns>
    protected abstract IQueryable<TEntity> Filter(TQuery query);

    /// <summary>
    ///     获取并过滤，返回 <see cref="IQueryable{T}" />
    /// </summary>
    /// <param name="query">输入的查询条件。</param>
    /// <returns>过滤后的 <see cref="IQueryable{T}" />。</returns>
    protected virtual Task<IQueryable<TEntity>> FilterAsync(TQuery query)
    {
        return Task.FromResult(Filter(query));
    }

    /// <summary>
    ///     投射结果，返回 <see cref="IQueryable{TView}" />。
    /// </summary>
    /// <param name="query">查询条件。</param>
    /// <param name="queryable">过滤并排序完成的 <see cref="IQueryable{T}" />。</param>
    /// <returns>投射好的 <see cref="IQueryable" />。</returns>
    protected virtual IQueryable<TView> ProjectToView(TQuery query, IQueryable<TEntity> queryable)
    {
        return queryable.ProjectToType<TView>();
    }

    /// <summary>
    ///     执行实际的查询。
    /// </summary>
    /// <param name="query">查询条件。</param>
    /// <param name="queryable">投射好的 <see cref="IQueryable{T}" /></param>
    /// <returns>查询结果。</returns>
    protected abstract Task<List<TView>> ToListAsync(TQuery query, IQueryable<TView> queryable);
}