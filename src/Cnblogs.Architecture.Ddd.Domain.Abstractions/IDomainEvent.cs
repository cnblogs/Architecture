using MediatR;

namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     领域事件标记。
/// </summary>
public interface IDomainEvent : INotification;
