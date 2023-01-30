using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义 <see cref="ICommand{TError}" /> 的实际处理逻辑。
/// </summary>
/// <typeparam name="TCommand">该 Handler 能够处理的命令类型。</typeparam>
/// <typeparam name="TView">命令返回的结果类型。</typeparam>
/// <typeparam name="TError">该 Handler 返回的错误码类型。</typeparam>
public interface ICommandHandler<TCommand, TView, TError> : IRequestHandler<TCommand, CommandResponse<TView, TError>>
    where TCommand : ICommand<TView, TError>
    where TError : Enumeration
{
}