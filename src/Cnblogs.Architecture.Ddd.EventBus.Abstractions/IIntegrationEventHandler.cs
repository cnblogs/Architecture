using MediatR;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
/// 集成事件处理器。
/// </summary>
/// <typeparam name="TEvent">集成事件。</typeparam>
public interface IIntegrationEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IntegrationEvent
{
}