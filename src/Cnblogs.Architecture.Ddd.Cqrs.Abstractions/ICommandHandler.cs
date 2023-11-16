using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Definitions of handler that handles <see cref="ICommand{TError}" />.
/// </summary>
/// <typeparam name="TCommand">The command type for this handler.</typeparam>
/// <typeparam name="TError">The error type for this handler.</typeparam>
public interface ICommandHandler<TCommand, TError> : IRequestHandler<TCommand, CommandResponse<TError>>
    where TCommand : ICommand<TError>
    where TError : Enumeration;
