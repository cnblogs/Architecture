using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义返回多个结果的查询。
/// </summary>
/// <typeparam name="TList">查询结果类型，通常是一个列表类型。</typeparam>
public interface IListQuery<TList> : IRequest<TList>
{
}