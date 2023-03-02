using Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;

/// <summary>
///     Hosed service to initialize clickhouse contexts.
/// </summary>
public class ClickhouseInitializeHostedService : IHostedService
{
    private readonly ClickhouseContextCollection _collection;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create a <see cref="ClickhouseInitializeHostedService"/>.
    /// </summary>
    /// <param name="collections">The contexts been registered.</param>
    /// <param name="serviceProvider">The provider for contexts.</param>
    public ClickhouseInitializeHostedService(
        IOptions<ClickhouseContextCollection> collections,
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _collection = collections.Value;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        foreach (var collectionContextType in _collection.ContextTypes)
        {
            var context = scope.ServiceProvider.GetRequiredService(collectionContextType) as ClickhouseDapperContext;
            context?.Init();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
