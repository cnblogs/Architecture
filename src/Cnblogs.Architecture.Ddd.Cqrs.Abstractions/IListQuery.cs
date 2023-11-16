using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a query returns a list of items.
/// </summary>
/// <typeparam name="TList">The list to return, usually a <see cref="List{T}"/>.</typeparam>
public interface IListQuery<TList> : IRequest<TList>;