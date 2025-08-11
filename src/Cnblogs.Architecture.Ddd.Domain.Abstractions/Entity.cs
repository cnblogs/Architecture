namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     实体基类。
/// </summary>
public abstract class Entity : EntityBase
{
    /// <summary>
    ///     实体软删除标记。
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    ///     实体创建时间。
    /// </summary>
    public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.Now;

    /// <summary>
    ///     实体最后更新时间。
    /// </summary>
    public DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.Now;

    /// <summary>
    ///     实体在更新前的操作，默认会更新 <see cref="Entity.DateUpdated" /> 属性。
    /// </summary>
    public override void BeforeUpdate()
    {
        DateUpdated = DateTimeOffset.Now;
    }
}

/// <summary>
///     实体基类。
/// </summary>
/// <typeparam name="TKey">实体的主键类型。</typeparam>
public abstract class Entity<TKey> : Entity, IEntity<TKey>
    where TKey : IComparable<TKey>
{
    private List<Func<TKey, IDomainEvent>>? _domainEventGenerator;

    /// <summary>
    ///     实体的主键。
    /// </summary>
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// 添加领域事件。
    /// </summary>
    /// <param name="generator">领域事件生成器。</param>
    public void AddDomainEvent(Func<TKey, IDomainEvent> generator)
    {
        _domainEventGenerator ??= [];
        _domainEventGenerator.Add(generator);
    }

    /// <inheritdoc />
    public override void ClearDomainEvents()
    {
        base.ClearDomainEvents();
        _domainEventGenerator?.Clear();
    }

    /// <inheritdoc />
    public override IReadOnlyCollection<IDomainEvent>? DomainEvents
    {
        get
        {
            var baseEvents = base.DomainEvents;
            var generatedEvents = GenerateDomainEvents();
            if (baseEvents != null && generatedEvents != null)
            {
                return generatedEvents.Concat(baseEvents).ToList();
            }

            return baseEvents ?? generatedEvents;
        }
    }

    private List<IDomainEvent>? GenerateDomainEvents() => _domainEventGenerator?.Select(x => x.Invoke(Id)).ToList();
}