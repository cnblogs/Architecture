namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// 领域事件来源，通常是实体 <see cref="Entity"/>。
/// </summary>
public interface IDomainEventSource
{
    /// <summary>
    /// 获取该来源包含的领域事件。
    /// </summary>
    /// <returns>领域事件。</returns>
    IReadOnlyCollection<IDomainEvent>? DomainEvents { get; }

    /// <summary>
    /// 添加领域事件。
    /// </summary>
    /// <param name="domainEvent">要添加的事件。</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// 清除领域事件。
    /// </summary>
    /// <param name="domainEvent">要清除的事件。</param>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// 清除领域事件。
    /// </summary>
    void ClearDomainEvents();
}