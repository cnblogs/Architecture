using Cnblogs.Architecture.Ddd.Domain.Abstractions;

// ReSharper disable once CheckNamespace
namespace MediatR;

/// <summary>
///     发布领域时间的拓展方法。
/// </summary>
public static class DispatchDomainEventExtensions
{
    /// <summary>
    ///     发布领域事件。
    /// </summary>
    /// <param name="mediator"><see cref="IMediator" />。</param>
    /// <param name="events">要发布的领域事件。</param>
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events ?? Enumerable.Empty<IDomainEvent>())
        {
            if (domainEvent == null)
            {
                continue;
            }

            if (domainEvent is INotification notification)
            {
                await mediator.Publish(notification);
            }
            else
            {
                await mediator.Publish(domainEvent);
            }
        }
    }
}