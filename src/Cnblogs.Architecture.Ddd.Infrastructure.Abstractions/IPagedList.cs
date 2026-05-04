using System.Collections;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

public interface IPagedList
{
    /// <summary>
    /// Get items of current page
    /// </summary>
    /// <returns></returns>
    IEnumerable GetItems();
}
