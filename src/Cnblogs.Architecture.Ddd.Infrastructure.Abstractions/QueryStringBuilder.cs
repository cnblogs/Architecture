using System.Web;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
/// 查询字符串构建器。
/// </summary>
public class QueryStringBuilder
{
    private readonly List<KeyValuePair<string, string>> _params = [];

    /// <summary>
    /// 创建一个 <see cref="QueryStringBuilder"/>。
    /// </summary>
    public QueryStringBuilder()
        : this(QueryStringNullHandleStrategy.Absent, QueryStringCollectionHandleStrategy.Repeat)
    {
    }

    /// <summary>
    /// 创建一个 <see cref="QueryStringBuilder"/>。
    /// </summary>
    /// <param name="stringNullHandleStrategy"><c>null</c>值处理方式。</param>
    /// <param name="collectionHandleStrategy">数组处理方式。</param>
    public QueryStringBuilder(
        QueryStringNullHandleStrategy stringNullHandleStrategy,
        QueryStringCollectionHandleStrategy collectionHandleStrategy)
    {
        NullHandleStrategy = stringNullHandleStrategy;
        CollectionHandleStrategy = collectionHandleStrategy;
    }

    /// <summary>
    /// <c>null</c> 值处理方式。
    /// </summary>
    public QueryStringNullHandleStrategy NullHandleStrategy { get; set; }

    /// <summary>
    /// 数组的处理方式。
    /// </summary>
    public QueryStringCollectionHandleStrategy CollectionHandleStrategy { get; set; }

    /// <summary>
    /// 添加分页参数。
    /// </summary>
    /// <param name="pagingParams">分页参数。</param>
    /// <returns></returns>
    public QueryStringBuilder AddPaging(PagingParams pagingParams)
        => AddPaging(pagingParams.PageIndex, pagingParams.PageSize);

    /// <summary>
    /// 添加分页参数。
    /// </summary>
    /// <param name="pageIndex">页码。</param>
    /// <param name="pageSize">分页大小。</param>
    public QueryStringBuilder AddPaging(int pageIndex, int pageSize)
        => Add(nameof(pageIndex), pageIndex).Add(nameof(pageSize), pageSize);

    /// <summary>
    /// 添加数组作为查询参数。
    /// </summary>
    /// <param name="key">键。</param>
    /// <param name="values">值。</param>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <returns></returns>
    public QueryStringBuilder AddRange<T>(string key, IEnumerable<T?>? values)
    {
        if (values is null)
        {
            return Add(key, null);
        }

        var index = 0;
        foreach (var x in values)
        {
            switch (CollectionHandleStrategy)
            {
                case QueryStringCollectionHandleStrategy.Repeat:
                    Add(key, x);
                    break;
                case QueryStringCollectionHandleStrategy.Index:
                    Add(key + $"[{index}]", x);
                    index++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return this;
    }

    /// <summary>
    /// 添加查询参数。
    /// </summary>
    /// <param name="key">键。</param>
    /// <param name="value">值。</param>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <returns></returns>
    public QueryStringBuilder Add<T>(string key, T? value)
        => value switch
        {
            null => Add(key, null),
            Enum e => Add(key, e.ToString("D")),
            _ => Add(key, value.ToString())
        };

    /// <summary>
    /// 添加查询参数。
    /// </summary>
    /// <param name="key">键。</param>
    /// <param name="value">值。</param>
    /// <exception cref="ArgumentOutOfRangeException">The <see cref="QueryStringBuilder.NullHandleStrategy"/>'s value is invalid</exception>
    /// <returns>当前 <see cref="QueryStringBuilder"/>。</returns>
    public QueryStringBuilder Add(string key, string? value)
    {
        if (value is null)
        {
            switch (NullHandleStrategy)
            {
                case QueryStringNullHandleStrategy.Absent:
                    return this;
                case QueryStringNullHandleStrategy.Empty:
                    value = string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _params.Add(new KeyValuePair<string, string>(key, value));
        return this;
    }

    /// <summary>
    /// 删除所有指定 <paramref name="key"/> 的查询字符串。
    /// </summary>
    /// <param name="key">键。</param>
    /// <returns></returns>
    public QueryStringBuilder RemoveAll(string key)
    {
        _params.RemoveAll(x => x.Key == key);
        return this;
    }

    /// <summary>
    /// 构建查询字符串，将以 '?' 开头。
    /// </summary>
    /// <returns></returns>
    public string Build()
    {
        return _params.Count == 0 ? string.Empty : "?" + BuildPartial();
    }

    /// <summary>
    /// 构建查询字符串，不包含开头的 '?'。
    /// </summary>
    /// <returns></returns>
    public string BuildPartial()
    {
        return _params.Count == 0
            ? string.Empty
            : string.Join(
                '&',
                _params.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}"));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Build();
    }
}