namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// 实体基类，仅在需要兼容旧版实体时才应该继承该类，否则请使用 <see cref="Entity{TKey}"/>
/// </summary>
public abstract class EntityBase : IDomainEventSource
{
    private List<IDomainEvent>? _events;

    /// <summary>
    ///     实体中的领域事件。
    /// </summary>
    public virtual IReadOnlyCollection<IDomainEvent>? DomainEvents => _events?.AsReadOnly();

    /// <summary>
    ///     添加领域事件。
    /// </summary>
    /// <param name="eventItem">领域事件。</param>
    public virtual void AddDomainEvent(IDomainEvent eventItem)
    {
        _events ??= new List<IDomainEvent>();
        _events.Add(eventItem);
    }

    /// <summary>
    ///     清空领域事件。
    /// </summary>
    public virtual void ClearDomainEvents()
    {
        _events?.Clear();
    }

    /// <summary>
    ///     删除领域事件。
    /// </summary>
    /// <param name="eventItem">需要删除的领域事件。</param>
    public virtual void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _events?.Remove(eventItem);
    }

    /// <summary>
    ///     实体在更新前的操作，默认什么也不做。
    /// </summary>
    public virtual void BeforeUpdate()
    {
        // do nothing by default
    }
}