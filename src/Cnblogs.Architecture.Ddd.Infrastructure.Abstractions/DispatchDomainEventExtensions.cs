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
        Exception? e = null;
        foreach (var domainEvent in events)
        {
            try
            {
                await mediator.Publish(domainEvent);
            }
            catch (Exception exception)
            {
                e ??= exception;
            }
        }

        if (e is not null)
        {
            throw e;
        }
    }
}