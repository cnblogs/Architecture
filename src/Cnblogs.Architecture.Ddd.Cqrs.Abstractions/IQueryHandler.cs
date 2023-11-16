using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents handler that handles <see cref="IQuery{TView}"/>.
/// </summary>
/// <typeparam name="TQuery">The <see cref="IQuery{TView}" /> type to handle.</typeparam>
/// <typeparam name="TView">The type of item to query.</typeparam>
public interface IQueryHandler<TQuery, TView> : IRequestHandler<TQuery, TView?>
    where TQuery : IQuery<TView>;