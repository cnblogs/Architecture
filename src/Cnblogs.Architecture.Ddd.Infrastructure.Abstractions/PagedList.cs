using System.Text.Json.Serialization;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     分页列表。
/// </summary>
/// <typeparam name="T">包含的元素类型。</typeparam>
public record PagedList<T>
{
    /// <summary>
    ///     创建一个空的 <see cref="PagedList{T}" /> 实例。
    /// </summary>
    public PagedList()
        : this(new List<T>())
    {
    }

    /// <summary>
    ///     创建一个新的分页列表。
    /// </summary>
    /// <param name="items">包含的元素。</param>
    /// <param name="pageIndex">页码。</param>
    /// <param name="pageSize">每页的元素个数。</param>
    /// <param name="totalCount">元素总数。</param>
    [JsonConstructor]
    public PagedList(IReadOnlyCollection<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>
    ///     创建一个新的分页列表。
    /// </summary>
    /// <param name="items">包含的元素。</param>
    /// <param name="pagingParams">分页参数。</param>
    /// <param name="totalCount">元素总数。</param>
    public PagedList(IReadOnlyCollection<T> items, PagingParams pagingParams, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
        var (pageIndex, pageSize) = pagingParams;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>
    ///     创建一个只有一页的分页列表。
    /// </summary>
    /// <param name="items">包含的元素。</param>
    public PagedList(IReadOnlyCollection<T> items)
    {
        Items = items;
        TotalCount = items.Count;
        PageIndex = 1;
        PageSize = items.Count;
    }

    /// <summary>
    ///     当前页的元素。
    /// </summary>
    public IReadOnlyCollection<T> Items { get; init; }

    /// <summary>
    ///     页码。
    /// </summary>
    public int PageIndex { get; init; }

    /// <summary>
    ///     每页元素数量。
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    ///     元素总数。
    /// </summary>
    public int TotalCount { get; init; }
}