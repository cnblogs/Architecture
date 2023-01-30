using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义返回分页结果的查询类型。
/// </summary>
/// <typeparam name="TElement">单个查询结果的类型。</typeparam>
public interface IPageableQuery<TElement> : IOrderedQuery<PagedList<TElement>>
{
    /// <summary>
    ///     分页参数。
    /// </summary>
    PagingParams? PagingParams { get; }
}