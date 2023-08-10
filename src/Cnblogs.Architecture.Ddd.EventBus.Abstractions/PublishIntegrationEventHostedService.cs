using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     The hosted service for publishing integration event at background.
/// </summary>
public sealed class PublishIntegrationEventHostedService : BackgroundService
{
    private readonly EventBusOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBuffer _eventBuffer;
    private readonly ILogger<PublishIntegrationEventHostedService> _logger;

    /// <summary>
    ///     Create a <see cref="PublishIntegrationEventHostedService"/>.
    /// </summary>
    /// <param name="options">The event bus options.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="eventBuffer">The buffer for integration events.</param>
    public PublishIntegrationEventHostedService(
        IOptions<EventBusOptions> options,
        IServiceProvider serviceProvider,
        ILogger<PublishIntegrationEventHostedService> logger,
        IEventBuffer eventBuffer)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventBuffer = eventBuffer;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Integration event publisher running.");
        var watch = new Stopwatch();
        using var timer = new PeriodicTimer(TimeSpan.FromMicroseconds(_options.Interval));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                watch.Restart();
                var beforeCount = _eventBuffer.Count;
                await PublishEventAsync();
                watch.Stop();
                var afterCount = _eventBuffer.Count;
                if (afterCount - beforeCount > 0)
                {
                    _logger.LogInformation(
                        "Published {PublishedEventCount} events in {Duration} ms, resting count: {RestingEventCount}",
                        beforeCount - afterCount,
                        watch.ElapsedMilliseconds,
                        afterCount);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Publish integration event failed, pending count: {Count}", _eventBuffer.Count);
            }
        }
    }

    private async Task PublishEventAsync()
    {
        if (_eventBuffer.Count == 0)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IEventBusProvider>();
        while (_eventBuffer.Count > 0)
        {
            var buffered = _eventBuffer.Peek();
            if (buffered is null)
            {
                return;
            }

            await provider.PublishAsync(buffered.Name, buffered.Event);
            _eventBuffer.Pop();
        }
    }
}
