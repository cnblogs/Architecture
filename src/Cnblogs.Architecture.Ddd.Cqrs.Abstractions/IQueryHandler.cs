using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义处理 <see cref="IQuery{TView}" /> 的逻辑。
/// </summary>
/// <typeparam name="TQuery">查询类型，需要继承 <see cref="IQuery{TView}" />。</typeparam>
/// <typeparam name="TView">结果类型。</typeparam>
public interface IQueryHandler<TQuery, TView> : IRequestHandler<TQuery, TView?>
    where TQuery : IQuery<TView>
{
}