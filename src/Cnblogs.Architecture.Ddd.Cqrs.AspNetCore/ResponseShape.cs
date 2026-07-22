using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     The shape of the payload returned by a CQRS endpoint, as observed by a generated service agent.
/// </summary>
public enum ResponseShape
{
    /// <summary>
    ///     The endpoint returns a single item (possibly <c>null</c> when not found).
    /// </summary>
    Item,

    /// <summary>
    ///     The endpoint returns a <see cref="List{T}"/>.
    /// </summary>
    List,

    /// <summary>
    ///     The endpoint returns a <see cref="PagedList{T}"/>.
    /// </summary>
    PagedList,

    /// <summary>
    ///     The endpoint returns no payload (a fire-and-forget command).
    /// </summary>
    None
}
