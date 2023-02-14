using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a handler for <see cref="IListQuery{TList}" />.
/// </summary>
/// <typeparam name="TQuery">The <see cref="IListQuery{TList}" /> been handled.</typeparam>
/// <typeparam name="TList">The result type of <typeparamref name="TQuery"/>.</typeparam>
public interface IListQueryHandler<TQuery, TList> : IRequestHandler<TQuery, TList>
    where TQuery : IListQuery<TList>
{
}