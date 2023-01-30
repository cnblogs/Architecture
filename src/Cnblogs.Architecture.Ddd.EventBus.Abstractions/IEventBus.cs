namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
/// 集成事件消息队列
/// </summary>
public interface IEventBus
{
    /// <summary>
    ///     发布事件。
    /// </summary>
    /// <param name="event">要发布的事件。</param>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : IntegrationEvent;

    /// <summary>
    ///     发布事件。
    /// </summary>
    /// <param name="eventName">要发布的事件名称。</param>
    /// <param name="event">事件内容。</param>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(string eventName, TEvent @event)
        where TEvent : IntegrationEvent;

    /// <summary>
    ///     尝试发布集成事件。
    /// </summary>
    /// <param name="event">要发布的集成事件。</param>
    /// <typeparam name="TEvent">集成事件类型。</typeparam>
    /// <returns>发布是否成功。</returns>
    Task<bool> TryPublishAsync<TEvent>(TEvent @event)
        where TEvent : IntegrationEvent;

    /// <summary>
    ///     尝试发布集成事件。
    /// </summary>
    /// <param name="eventName">集成事件名称。</param>
    /// <param name="event">集成事件。</param>
    /// <typeparam name="TEvent">集成事件类型。</typeparam>
    /// <returns>发布是否成功。</returns>
    Task<bool> TryPublishAsync<TEvent>(string eventName, TEvent @event)
        where TEvent : IntegrationEvent;

    /// <summary>
    ///     接收并处理集成事件。
    /// </summary>
    /// <param name="receivedEvent">收到的集成事件。</param>
    /// <typeparam name="TEvent">集成事件类型。</typeparam>
    /// <returns></returns>
    Task ReceiveAsync<TEvent>(TEvent receivedEvent)
        where TEvent : IntegrationEvent;

    /// <summary>
    ///     当前上下文的 TraceId。
    /// </summary>
    Guid? TraceId { get; set; }
}