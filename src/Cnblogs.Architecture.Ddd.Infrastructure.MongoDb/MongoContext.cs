using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using MongoDB.Driver;

namespace Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

/// <summary>
///     用于与数据库交互的上下文，使用方法类似于 DbContext
/// </summary>
public abstract class MongoContext
{
    private readonly IMongoContextOptions _options;
    private readonly IMongoDatabase _database;
    private static readonly BulkWriteOptions BulkWriteOptions = new() { IsOrdered = true };

    /// <summary>
    ///     创建一个 MongoContext
    /// </summary>
    /// <param name="options"><see cref="MongoContextOptions"/>.</param>
    public MongoContext(IMongoContextOptions options)
    {
        _options = options;
        _database = _options.GetDatabase();
    }

    /// <summary>
    ///     获取实体对应的 <see cref="IMongoCollection{TEntity}"/>。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <returns></returns>
    public IMongoCollection<TEntity> Set<TEntity>()
        => _database.GetCollection<TEntity>(_options.ResolveCollection<TEntity>());

    /// <summary>
    ///     初始化 MongoContext，进行 Bson 配置等。
    /// </summary>
    public void Init()
    {
        var builder = new MongoModelBuilder(_options);
        ConfigureModels(builder);
    }

    /// <summary>
    ///     会在程序启动时调用，配置实体映射。
    /// </summary>
    /// <param name="builder">配置器。</param>
    protected abstract void ConfigureModels(MongoModelBuilder builder);

    internal async Task<int> BulkWriteWithTransactionAsync<TEntity, TKey>(
        Dictionary<TKey, TEntity>? toAdd,
        Dictionary<TKey, TEntity>? toUpdate,
        Dictionary<TKey, TEntity>? toDelete,
        CancellationToken cancellationToken = default)
        where TEntity : IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        var totalCount = (toAdd?.Count ?? 0) + (toUpdate?.Count ?? 0) + ((toDelete?.Count ?? 0) > 0 ? 1 : 0);
        if (totalCount == 0)
        {
            return 0;
        }

        using var session = await _database.Client.StartSessionAsync(null, cancellationToken);
        var collection = Set<TEntity>();
        try
        {
            session.StartTransaction();
            var operations = new List<WriteModel<TEntity>>(totalCount);

            if (toAdd is { Count: > 0 })
            {
                operations.AddRange(toAdd.Values.Select(entity => new InsertOneModel<TEntity>(entity)));
            }

            if (toUpdate is { Count: > 0 })
            {
                operations.AddRange(
                    toUpdate.Values.Select(
                        e => new ReplaceOneModel<TEntity>(Builders<TEntity>.Filter.Eq(x => x.Id, e.Id), e)));
            }

            if (toDelete is { Count: > 0 })
            {
                operations.Add(new DeleteManyModel<TEntity>(Builders<TEntity>.Filter.In(x => x.Id, toDelete.Keys)));
            }

            await collection.BulkWriteAsync(
                session,
                operations,
                BulkWriteOptions,
                cancellationToken: cancellationToken);
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }

        await session.CommitTransactionAsync(cancellationToken);
        return totalCount;
    }
}