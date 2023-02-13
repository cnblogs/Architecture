namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a <see cref="IListQuery{TList}"/> with ordered results.
/// </summary>
/// <typeparam name="TList">The querying type, usually a list type.</typeparam>
public interface IOrderedQuery<TList> : IListQuery<TList>
{
    /// <summary>
    ///     The string indicates the order.
    /// </summary>
    /// <example>Order by string can be like "-dateadded"(order by dateadded desc) or "id"(order by id asc).</example>
    string? OrderByString { get; }
}