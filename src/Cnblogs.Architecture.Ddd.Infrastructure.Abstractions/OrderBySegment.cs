using System.Linq.Expressions;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     排序参数。
/// </summary>
/// <param name="IsDesc">是否倒序。</param>
/// <param name="PropertyExpression">排序列对应的属性。</param>
public record OrderBySegment(bool IsDesc, LambdaExpression PropertyExpression)
{
    /// <summary>
    ///     创建一个新的 <see cref="OrderBySegment" />。
    /// </summary>
    /// <param name="isDesc">是否倒序。</param>
    /// <param name="exp">排序列对应的属性。</param>
    /// <typeparam name="TEntity">要排序的实体。</typeparam>
    /// <typeparam name="TProperty">排序列的类型。</typeparam>
    /// <returns><see cref="OrderBySegment" />。</returns>
    public static OrderBySegment Create<TEntity, TProperty>(bool isDesc, Expression<Func<TEntity, TProperty>> exp)
    {
        return new OrderBySegment(isDesc, exp);
    }

    /// <summary>
    ///     获得 <see cref="OrderBySegment" /> 的字符串表示形式。
    /// </summary>
    /// <param name="segment">字符串来源。</param>
    /// <returns></returns>
    public static string ToDisplayString(OrderBySegment segment)
    {
        return (segment.IsDesc ? "-" : string.Empty) + segment.PropertyExpression.Body;
    }

    /// <summary>
    ///     获得一组 <see cref="OrderBySegment" /> 的字符串表示形式。
    /// </summary>
    /// <param name="segments">一组 <see cref="OrderBySegment" />。</param>
    /// <returns></returns>
    public static string ToDisplayString(IEnumerable<OrderBySegment> segments)
    {
        return string.Join(',', segments.Select(ToDisplayString));
    }
}