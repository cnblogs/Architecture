using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents the handler for <see cref="IPageableQuery{TElement}" />.
/// </summary>
/// <typeparam name="TQuery">The <see cref="IPageableQuery{TElement}" /> to handle.</typeparam>
/// <typeparam name="TView">The type for each item in <see cref="PagedList{T}"/>.</typeparam>
public interface IPageableQueryHandler<TQuery, TView> : IListQueryHandler<TQuery, PagedList<TView>>
    where TQuery : IPageableQuery<TView>;