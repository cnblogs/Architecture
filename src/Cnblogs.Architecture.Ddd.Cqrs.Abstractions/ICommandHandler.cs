using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义 <see cref="ICommand{TError}" /> 的实际处理逻辑。
/// </summary>
/// <typeparam name="TCommand">该 Handler 能够处理的命令类型。</typeparam>
/// <typeparam name="TError">该 Handler 返回的错误码类型。</typeparam>
public interface ICommandHandler<TCommand, TError> : IRequestHandler<TCommand, CommandResponse<TError>>
    where TCommand : ICommand<TError>
    where TError : Enumeration
{
}