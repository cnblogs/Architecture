using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.TestIntegrationEvents;
using static Cnblogs.Architecture.IntegrationTestProject.Constants;

namespace Cnblogs.Architecture.IntegrationTestProject.EventHandlers;

public partial class TestIntegrationEventHandler(ILogger<TestIntegrationEventHandler> logger)
    : IIntegrationEventHandler<TestIntegrationEvent>,
        IIntegrationEventHandler<BlogPostCreatedIntegrationEvent>
{
    private readonly ILogger _logger = logger;

    public Task Handle(TestIntegrationEvent notification, CancellationToken cancellationToken)
    {
        LogHandledIntegrationEventEvent(notification);
        return Task.CompletedTask;
    }

    public Task Handle(BlogPostCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        LogHandledIntegrationEventEvent(notification);
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, LogTemplates.HandledIntegratonEvent)]
    partial void LogHandledIntegrationEventEvent(object @event);
}
