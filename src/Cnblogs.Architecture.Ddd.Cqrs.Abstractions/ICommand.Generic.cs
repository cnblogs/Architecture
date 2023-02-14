using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Definitions of command in CQRS.
/// </summary>
/// <typeparam name="TView">The result type for command.</typeparam>
/// <typeparam name="TError">The error code type when command failed.</typeparam>
public interface ICommand<TView, TError> : IRequest<CommandResponse<TView, TError>>
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