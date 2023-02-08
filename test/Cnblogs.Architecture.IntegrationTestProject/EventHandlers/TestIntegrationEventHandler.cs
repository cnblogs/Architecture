using System.Diagnostics;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.TestIntegrationEvents;
using MediatR;

namespace Cnblogs.Architecture.IntegrationTestProject.EventHandlers;

public class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public TestIntegrationEventHandler(IHttpContextAccessor httpContextAccessor, ILogger<TestIntegrationEventHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task Handle(TestIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        context?.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(Constants.IntegrationEventIdHeaderName, notification.Id.ToString());
            return Task.CompletedTask;
        });

        _logger.LogInformation("Handled integration event {event}.", notification);

        return Task.CompletedTask;
    }
}
