using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;

/// <summary>
///     查询用的扩展方法。
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    ///     对 <see cref="IQueryable{T}" /> 应用多个 Include 语句。
    /// </summary>
    /// <param name="query">源查询。</param>
    /// <param name="includes">Include 表达式数组。</param>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <returns></returns>
    public static IQueryable<TEntity> AggregateIncludes<TEntity>(
        this IQueryable<TEntity> query,
        IEnumerable<Expression<Func<TEntity, object?>>> includes)
        where TEntity : class
    {
        return includes.Aggregate(query, (queryable, include) => queryable.Include(include));
    }
}