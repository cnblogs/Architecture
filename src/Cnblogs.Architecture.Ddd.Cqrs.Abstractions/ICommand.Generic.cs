using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义 CQRS 中的命令相关的行为。
/// </summary>
/// <typeparam name="TView">命令执行成功时返回的结果。</typeparam>
/// <typeparam name="TError">命令失败时返回的错误码类型。</typeparam>
public interface ICommand<TView, TError> : IRequest<CommandResponse<TView, TError>>
    where TError : Enumeration
{
    /// <summary>
    ///     命令是否只进行验证。
    /// </summary>
    public bool ValidateOnly { get; }
}