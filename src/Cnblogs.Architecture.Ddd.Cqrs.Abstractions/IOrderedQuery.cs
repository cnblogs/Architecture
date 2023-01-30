namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义返回已排序的多个结果的查询。
/// </summary>
/// <typeparam name="TList">查询结果类型，通常是列表类型。</typeparam>
public interface IOrderedQuery<TList> : IListQuery<TList>
{
    /// <summary>
    ///     排序字符串。
    /// </summary>
    string? OrderByString { get; }
}