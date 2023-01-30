using MongoDB.Driver;

namespace Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

/// <summary>
///     MongoContext 依赖的配置项。
/// </summary>
public interface IMongoContextOptions
{
    /// <summary>
    ///     通过设置获取对应的 <see cref="IMongoDatabase"/>。
    /// </summary>
    /// <returns></returns>
    IMongoDatabase GetDatabase();

    /// <summary>
    ///     配置实体到数据库 Collection 的映射。
    /// </summary>
    /// <param name="collectionName">Collection 名称。</param>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    void MapEntity<TEntity>(string collectionName);

    /// <summary>
    ///     获取实体配置的 Collection 名称。
    /// </summary>
    /// <typeparam name="TEntity">Collection 名称。</typeparam>
    /// <returns></returns>
    string ResolveCollection<TEntity>();
}