using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义 <see cref="IListQuery{TList}" /> 的处理逻辑。
/// </summary>
/// <typeparam name="TQuery">该 Handler 能够处理的 <see cref="IListQuery{TList}" /> 类型。</typeparam>
/// <typeparam name="TList">查询结果类型。</typeparam>
public interface IListQueryHandler<TQuery, TList> : IRequestHandler<TQuery, TList>
    where TQuery : IListQuery<TList>
{
}