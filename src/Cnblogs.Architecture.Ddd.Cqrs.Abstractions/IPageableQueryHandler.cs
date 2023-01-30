using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义处理分页查询的逻辑。
/// </summary>
/// <typeparam name="TQuery">查询类型，需要继承 <see cref="IPageableQuery{TElement}" />。</typeparam>
/// <typeparam name="TView">单个查询结果类型，将返回 IPagedList&lt;TView&gt;。</typeparam>
public interface IPageableQueryHandler<TQuery, TView> : IListQueryHandler<TQuery, PagedList<TView>>
    where TQuery : IPageableQuery<TView>
{
}