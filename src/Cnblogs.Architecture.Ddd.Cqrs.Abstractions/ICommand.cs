using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义 CQRS 中的命令相关的行为。
/// </summary>
/// <typeparam name="TError">命令失败时返回的错误码类型。</typeparam>
public interface ICommand<TError> : IRequest<CommandResponse<TError>>
    where TError : Enumeration
{
    /// <summary>
    ///     命令是否只执行验证。
    /// </summary>
    public bool ValidateOnly { get; }
}