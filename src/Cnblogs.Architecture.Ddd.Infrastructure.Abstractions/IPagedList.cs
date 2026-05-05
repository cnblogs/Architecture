using System.Collections;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
/// Marker interface for <see cref="PagedList{T}"/>
/// </summary>
public interface IPagedList
{
    /// <summary>
    /// Get items of current page
    /// </summary>
    /// <returns></returns>
    IEnumerable GetItems();
}
