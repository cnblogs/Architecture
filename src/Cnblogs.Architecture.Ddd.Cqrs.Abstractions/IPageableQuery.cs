using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a <see cref="IOrderedQuery{TList}"/> with paged results.
/// </summary>
/// <typeparam name="TElement">The type for each item in results.</typeparam>
public interface IPageableQuery<TElement> : IOrderedQuery<PagedList<TElement>>
{
    /// <summary>
    ///     The paging parameters, include page index and page size.
    /// </summary>
    PagingParams? PagingParams { get; }
}