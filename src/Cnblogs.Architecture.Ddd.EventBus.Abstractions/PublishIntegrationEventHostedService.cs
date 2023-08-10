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
        _logger.LogInformation("Integration event publisher running");
        var watch = new Stopwatch();
        var failureCounter = 0;
        var successCounter = 0;
        using var normalTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.Interval));
        using var failedTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.DowngradeInterval));
        var currentTimer = normalTimer;
        var downgraded = false;
        while (await currentTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                watch.Restart();
                var sent = await PublishEventAsync();
                watch.Stop();
                var afterCount = _eventBuffer.Count;
                if (sent > 0)
                {
                    successCounter++;
                    _logger.LogInformation(
                        "Published {PublishedEventCount} events in {Duration} ms, resting count: {RestingEventCount}",
                        sent,
                        watch.ElapsedMilliseconds,
                        afterCount);
                }
            }
            catch (Exception e)
            {
                failureCounter++;
                _logger.LogWarning(
                    e,
                    "Publish integration event failed, pending count: {Count}, failure count: {FailureCount}",
                    _eventBuffer.Count,
                    failureCounter);
            }

            if (downgraded == false && failureCounter >= _options.FailureCountBeforeDowngrade)
            {
                _logger.LogError("Integration event publisher downgraded");
                downgraded = true;
                currentTimer = failedTimer;
                successCounter = 0;
            }

            if (downgraded && successCounter > _options.SuccessCountBeforeRecover)
            {
                downgraded = false;
                currentTimer = normalTimer;
                failureCounter = 0;
                _logger.LogWarning("Integration event publisher recovered from downgrade");
            }
        }
    }

    private async Task<int> PublishEventAsync()
    {
        if (_eventBuffer.Count == 0)
        {
            return 0;
        }

        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IEventBusProvider>();
        var publishedEventCount = 0;
        while (_eventBuffer.Count > 0 && publishedEventCount != _options.MaximumBatchSize)
        {
            var buffered = _eventBuffer.Peek();
            if (buffered is null)
            {
                break;
            }

            await provider.PublishAsync(buffered.Name, buffered.Event);
            _eventBuffer.Pop();
            publishedEventCount++;
        }

        return publishedEventCount;
    }
}
