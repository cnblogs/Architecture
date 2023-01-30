using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义获取单个结果的查询。
/// </summary>
/// <typeparam name="TView">结果类型。</typeparam>
public interface IQuery<TView> : IRequest<TView?>
{
}