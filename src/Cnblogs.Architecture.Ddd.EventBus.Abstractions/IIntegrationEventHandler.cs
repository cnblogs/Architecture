using MediatR;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
/// 集成事件处理器。
/// </summary>
/// <typeparam name="TEvent">集成事件。</typeparam>
#pragma warning disable SA1402 // File may only contain a single type
public interface IIntegrationEventHandler<TEvent> : INotificationHandler<TEvent>, IIntegrationEventHandler
#pragma warning restore SA1402 // File may only contain a single type
    where TEvent : IntegrationEvent
{
}

/// <summary>
/// The non-generic interface as a generic type constraint
/// </summary>
public interface IIntegrationEventHandler
{
}
