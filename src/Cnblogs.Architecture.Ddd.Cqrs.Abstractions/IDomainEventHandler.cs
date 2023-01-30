using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     领域事件处理器。
/// </summary>
/// <typeparam name="TDomainEvent">要订阅的领域事件。</typeparam>
public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : DomainEvent
{
}