using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Mapster;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Base class for implementing <see cref="IPageableQueryHandler{TQuery,TView}" />.
/// </summary>
/// <typeparam name="TQuery">The type of query.</typeparam>
/// <typeparam name="TEntity">The type of entity to query.</typeparam>
/// <typeparam name="TView">The type of projected view model.</typeparam>
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
                return new PagedList<TView>([], request.PagingParams, totalCount);
            }

            ordered = ordered.Paging(request.PagingParams);
        }

        var items = await ToListAsync(request, ProjectToView(request, ordered));
        return request.PagingParams == null
            ? new PagedList<TView>(items)
            : new PagedList<TView>(items, request.PagingParams, totalCount);
    }

    /// <summary>
    ///     Query for total count.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="queryable">Filtered <see cref="IQueryable{T}" />.</param>
    /// <returns>The total count of items.</returns>
    protected abstract Task<int> CountAsync(TQuery query, IQueryable<TEntity> queryable);

    /// <summary>
    ///     The default order by field, used when <see cref="OrderBySegment" /> is not present.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="queryable"><see cref="IQueryable{TEntity}" /> returned by <see cref="Filter" />.</param>
    /// <returns>Ordered <see cref="IQueryable{T}" />.</returns>
    protected abstract IQueryable<TEntity> DefaultOrderBy(TQuery query, IQueryable<TEntity> queryable);

    /// <summary>
    ///     Create queryable and apply filter, return filtered <see cref="IQueryable{T}" />.
    /// </summary>
    /// <param name="query">The query parameter.</param>
    /// <returns>Filtered <see cref="IQueryable{T}" />.</returns>
    protected abstract IQueryable<TEntity> Filter(TQuery query);

    /// <summary>
    ///     Create queryable and apply filter asynchronously, return filtered <see cref="IQueryable{T}" />.
    /// </summary>
    /// <param name="query">The query parameter.</param>
    /// <returns>Filtered <see cref="IQueryable{T}" />.</returns>
    protected virtual Task<IQueryable<TEntity>> FilterAsync(TQuery query)
    {
        return Task.FromResult(Filter(query));
    }

    /// <summary>
    ///     Project item to view model, return projected <see cref="IQueryable{TView}" />.
    /// </summary>
    /// <param name="query">The query parameter.</param>
    /// <param name="queryable">Filtered and ordered <see cref="IQueryable{T}" />.</param>
    /// <returns>Projected <see cref="IQueryable" />.</returns>
    protected virtual IQueryable<TView> ProjectToView(TQuery query, IQueryable<TEntity> queryable)
    {
        return queryable.ProjectToType<TView>();
    }

    /// <summary>
    ///     Execute query and projections, get the actual results.
    /// </summary>
    /// <param name="query">The query parameter.</param>
    /// <param name="queryable">Projected <see cref="IQueryable{T}" />.</param>
    /// <returns>The query result.</returns>
    protected abstract Task<List<TView>> ToListAsync(TQuery query, IQueryable<TView> queryable);
}
