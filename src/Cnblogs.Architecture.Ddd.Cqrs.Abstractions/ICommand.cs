using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Definition for command.
/// </summary>
/// <typeparam name="TError">The error type when command execution failed.</typeparam>
public interface ICommand<TError> : IRequest<CommandResponse<TError>>
    where TError : Enumeration
{
    /// <summary>
    ///     Only execute validation logic.
    /// </summary>
    /// <remarks>
    ///     This logic must be implemented manually in command handler and not guaranteed by framework.
    /// </remarks>
    public bool ValidateOnly { get; }
}