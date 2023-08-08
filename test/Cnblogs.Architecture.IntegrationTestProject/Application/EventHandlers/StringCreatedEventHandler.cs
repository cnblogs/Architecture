using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Domain.Events;
using Cnblogs.Architecture.TestIntegrationEvents;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.EventHandlers;

public class StringCreatedEventHandler : IDomainEventHandler<StringCreatedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public StringCreatedEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <inheritdoc />
    public async Task Handle(StringCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TestIntegrationEvent(Guid.NewGuid(), DateTimeOffset.Now, notification.Data));
    }
}
