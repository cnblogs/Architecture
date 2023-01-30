using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using MongoDB.Bson.Serialization;

namespace Cnblogs.Architecture.Ddd.Cqrs.MongoDb;

/// <summary>
///     MongoDb 初始化配置服务。
/// </summary>
public class MongoConfigureService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MongoContextCollection _contextCollection;

    /// <summary>
    ///     创建一个 Bson 初始化服务。
    /// </summary>
    /// <param name="contextCollection">MongoContext 列表。</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/>。</param>
    public MongoConfigureService(IOptions<MongoContextCollection> contextCollection, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _contextCollection = contextCollection.Value;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        BsonClassMap.RegisterClassMap<EntityBase>(
            cm =>
            {
                cm.AutoMap();
                cm.UnmapProperty(x => x.DomainEvents);
            });
        BsonClassMap.RegisterClassMap<Entity>();

        foreach (var contextCollectionContextType in _contextCollection.MongoContexts)
        {
            if (scope.ServiceProvider.GetRequiredService(contextCollectionContextType) is not MongoContext context)
            {
                throw new InvalidOperationException("input context is not MongoContext!");
            }

            context.Init();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}