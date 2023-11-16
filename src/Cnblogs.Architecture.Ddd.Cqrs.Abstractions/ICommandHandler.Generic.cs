using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Definitions of handler that handles <see cref="ICommand{TView, TError}" />。
/// </summary>
/// <typeparam name="TCommand">The command type for this handler.</typeparam>
/// <typeparam name="TView">The result type for this handler.</typeparam>
/// <typeparam name="TError">The error type for this handler.</typeparam>
public interface ICommandHandler<TCommand, TView, TError> : IRequestHandler<TCommand, CommandResponse<TView, TError>>
    where TCommand : ICommand<TView, TError>
    where TError : Enumeration;