using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents query for single item.
/// </summary>
/// <typeparam name="TView">The type of item to query.</typeparam>
public interface IQuery<TView> : IRequest<TView?>
{
}