namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
/// <see cref="QueryStringBuilder"/> 数组处理方式。
/// </summary>
public enum QueryStringCollectionHandleStrategy
{
    /// <summary>
    /// 添加多个键相同的查询参数，例如 <c>?key=1&amp;key=2&amp;key=3</c>
    /// </summary>
    Repeat = 0,

    /// <summary>
    /// 添加带序号的查询参数，例如 <c>?key[1]=1&amp;key[2]=2&amp;key[3]=3</c>
    /// </summary>
    Index = 1,
}