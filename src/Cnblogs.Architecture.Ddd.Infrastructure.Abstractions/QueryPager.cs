namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     <see cref="PagingParams" /> 用于 <see cref="IQueryable{T}" /> 的扩展方法。
/// </summary>
public static class QueryPager
{
    /// <summary>
    ///     使用 <see cref="PagingParams" /> 进行分页。
    /// </summary>
    /// <param name="queryable">要分页的列表。</param>
    /// <param name="pagingParams">分页参数。</param>
    /// <typeparam name="T">被分页的元素类型。</typeparam>
    /// <returns>分页后的列表。</returns>
    public static IQueryable<T> Paging<T>(this IQueryable<T> queryable, PagingParams pagingParams)
    {
        (var pageIndex, var pageSize) = pagingParams;
        return queryable.Paging(pageIndex, pageSize);
    }

    /// <summary>
    ///     对查询进行分页。
    /// </summary>
    /// <param name="queryable">被分页的查询。</param>
    /// <param name="pageIndex">要查询的页码。</param>
    /// <param name="pageSize">要查询的元素数。</param>
    /// <typeparam name="T">被查询的元素类型。</typeparam>
    /// <returns>分页后的查询。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageIndex" /> 小于等于 0。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize" /> 小于等于 0。</exception>
    public static IQueryable<T> Paging<T>(this IQueryable<T> queryable, int pageIndex, int pageSize)
    {
        if (pageIndex <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageIndex),
                pageIndex,
                nameof(pageIndex) + "must be positive");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                nameof(pageSize) + "must be positive");
        }

        return pageIndex == 1
            ? queryable.Take(pageSize)
            : queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);
    }
}