using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.TestIntegrationEvents;
using MediatR;
using System.Diagnostics;

namespace Cnblogs.Architecture.IntegrationTestProject.EventHandlers;

public class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestIntegrationEventHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task Handle(TestIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        context?.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(Constants.IntegrationEventIdHeaderName, notification.Id.ToString());
            return Task.CompletedTask;
        });

        Debug.WriteLine($"notification message: " + notification.Message);

        return Task.CompletedTask;
    }
}
