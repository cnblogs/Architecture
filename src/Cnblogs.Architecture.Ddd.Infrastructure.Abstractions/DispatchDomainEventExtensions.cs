using Cnblogs.Architecture.Ddd.Domain.Abstractions;

// ReSharper disable once CheckNamespace
namespace MediatR;

/// <summary>
///     发布领域事件的拓展方法。
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
        List<Exception>? exceptions = null;
        foreach (var domainEvent in events)
        {
            try
            {
                await mediator.Publish(domainEvent);
            }
            catch (Exception exception)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(exception);
            }
        }

        if (exceptions?.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}
