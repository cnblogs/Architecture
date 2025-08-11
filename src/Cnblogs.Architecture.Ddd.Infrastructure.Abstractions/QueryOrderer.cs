using System.Linq.Expressions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

// ReSharper disable once CheckNamespace
namespace System.Linq;

/// <summary>
///     <see cref="OrderBySegment" /> 用于 <see cref="IQueryable{T}" /> 的扩展方法。
/// </summary>
public static class QueryOrderer
{
    private static string GetOrderByMethodName(bool isDesc)
    {
        return isDesc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
    }

    private static string GetThenByMethodName(bool isDesc)
    {
        return isDesc ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);
    }

    /// <summary>
    ///     使用 <see cref="OrderBySegment" /> 进行排序。
    /// </summary>
    /// <param name="queryable">要排序的列表。</param>
    /// <param name="segment">排序参数。</param>
    /// <typeparam name="TSource">要排序的类型。</typeparam>
    /// <returns>排好序的 <see cref="IQueryable{T}" />。</returns>
    public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> queryable, OrderBySegment segment)
    {
        var (isDesc, exp) = segment;
        var method = GetOrderByMethodName(isDesc);
        Type[] types = [queryable.ElementType, exp.Body.Type];
        var rs = Expression.Call(typeof(Queryable), method, types, queryable.Expression, exp);
        return queryable.Provider.CreateQuery<TSource>(rs);
    }

    /// <summary>
    ///     使用 <see cref="OrderBySegment" /> 进行排序。
    /// </summary>
    /// <param name="queryable">要排序的列表。</param>
    /// <param name="segments">排序参数。</param>
    /// <typeparam name="TSource">要排序的类型。</typeparam>
    /// <returns>排好序的 <see cref="IQueryable{T}" />。</returns>
    public static IQueryable<TSource> OrderBy<TSource>(
        this IQueryable<TSource> queryable,
        IReadOnlyCollection<OrderBySegment> segments)
    {
        return segments.Count == 1
            ? queryable.OrderBy(segments.First())
            : queryable.OrderBy((IEnumerable<OrderBySegment>)segments);
    }

    /// <summary>
    ///     使用 <see cref="OrderBySegment" /> 进行排序。
    /// </summary>
    /// <param name="queryable">要排序的列表。</param>
    /// <param name="segments">排序参数。</param>
    /// <typeparam name="TSource">要排序的类型。</typeparam>
    /// <returns>排好序的 <see cref="IQueryable{T}" />。</returns>
    /// <returns></returns>
    public static IQueryable<TSource> OrderBy<TSource>(
        this IQueryable<TSource> queryable,
        IEnumerable<OrderBySegment> segments)
    {
        var isFirst = true;
        foreach (var (isDesc, exp) in segments)
        {
            var method = isFirst ? GetOrderByMethodName(isDesc) : GetThenByMethodName(isDesc);
            Type[] types = [queryable.ElementType, exp.Body.Type];
            var rs = Expression.Call(typeof(Queryable), method, types, queryable.Expression, exp);
            queryable = queryable.Provider.CreateQuery<TSource>(rs);
            isFirst = false;
        }

        return queryable;
    }
}