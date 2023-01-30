using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using MongoDB.Bson.Serialization;

namespace Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

/// <summary>
///     配置 MongoDb 实体到表的映射。
/// </summary>
public class MongoModelBuilder
{
    private readonly IMongoContextOptions _options;

    /// <summary>
    ///     创建一个 MongoModelBuilder。
    /// </summary>
    /// <param name="options">正在配置的 <see cref="MongoContextOptions{TContext}"/>。</param>
    public MongoModelBuilder(IMongoContextOptions options)
    {
        _options = options;
    }

    /// <summary>
    ///     配置实体类型。
    /// </summary>
    /// <param name="collectionName">表名称。</param>
    /// <param name="mapConfigure">Bson 配置。</param>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public void Entity<TEntity>(string collectionName, Action<BsonClassMap<TEntity>>? mapConfigure = null)
        where TEntity : EntityBase
    {
        _options.MapEntity<TEntity>(collectionName);
        var map = BsonClassMap.RegisterClassMap<TEntity>();
        map.SetIgnoreExtraElements(true);
        mapConfigure?.Invoke(map);
    }

    /// <summary>
    ///     配置实体类型。
    /// </summary>
    /// <param name="collectionName">表名称。</param>
    /// <param name="mapConfigure">Bson 配置。</param>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <typeparam name="TKey">键类型。</typeparam>
    public void Entity<TEntity, TKey>(string collectionName, Action<BsonClassMap<TEntity>>? mapConfigure = null)
        where TEntity : Entity<TKey>
        where TKey : IComparable<TKey>
    {
        _options.MapEntity<TEntity>(collectionName);
        if (BsonClassMap.IsClassMapRegistered(typeof(Entity<TKey>)) == false)
        {
            BsonClassMap.RegisterClassMap<Entity<TKey>>(cm => cm.MapIdProperty(x => x.Id));
        }

        var map = BsonClassMap.RegisterClassMap<TEntity>();
        map.SetIgnoreExtraElements(true);
        mapConfigure?.Invoke(map);
    }
}