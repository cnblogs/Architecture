using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.TestIntegrationEvents;
using static Cnblogs.Architecture.IntegrationTestProject.Constants;

namespace Cnblogs.Architecture.IntegrationTestProject.EventHandlers;

public class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>,
    IIntegrationEventHandler<BlogPostCreatedIntegrationEvent>
{
    private readonly ILogger _logger;

    public TestIntegrationEventHandler(ILogger<TestIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TestIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplates.HandledIntegratonEvent, notification);

        return Task.CompletedTask;
    }

    public Task Handle(BlogPostCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplates.HandledIntegratonEvent, notification);

        return Task.CompletedTask;
    }
}