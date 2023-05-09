namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     领域事件基类。
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    ///     领域事件生成时间。
    /// </summary>
    public DateTimeOffset CreateAt { get; init; } = DateTimeOffset.Now;
}