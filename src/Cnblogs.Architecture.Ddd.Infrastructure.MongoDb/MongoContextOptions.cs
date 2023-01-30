using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

/// <summary>
///     MongoContext 的配置文件。
/// </summary>
/// <typeparam name="TContext">要配置的 MongoContext。</typeparam>
public class MongoContextOptions<TContext> : MongoContextOptions
    where TContext : MongoContext
{
    /// <summary>
    ///     创建一个 <see cref="MongoContextOptions{TContext}"/> 实例。
    /// </summary>
    /// <param name="connectionString">连接字符串。</param>
    /// <param name="databaseName">数据库名称。</param>
    public MongoContextOptions(string connectionString, string databaseName)
        : base(connectionString, databaseName)
    {
    }
}

/// <summary>
///     MongoContext 的配置。
/// </summary>
public abstract class MongoContextOptions : IMongoContextOptions
{
    private readonly MongoClientSettings _settings;
    private readonly Dictionary<Type, string> _collectionMap = new();
    private readonly string _databaseName;
    private IMongoClient? _mongoClient;

    /// <summary>
    ///     创建一个 <see cref="MongoContextOptions{TContext}"/> 实例。
    /// </summary>
    /// <param name="connectionString">连接字符串。</param>
    /// <param name="databaseName">数据库名称。</param>
    protected MongoContextOptions(string connectionString, string databaseName)
    {
        _databaseName = databaseName;
        _settings = MongoClientSettings.FromConnectionString(connectionString);
        _settings.LinqProvider = LinqProvider.V3;
    }

    /// <inheritdoc />
    public IMongoDatabase GetDatabase() => GetClient().GetDatabase(_databaseName);

    /// <inheritdoc />
    public void MapEntity<TEntity>(string collectionName)
    {
        var type = typeof(TEntity);
        if (_collectionMap.ContainsKey(type))
        {
            _collectionMap[type] = collectionName;
        }
        else
        {
            _collectionMap.Add(type, collectionName);
        }
    }

    /// <inheritdoc />
    public string ResolveCollection<TEntity>() => _collectionMap[typeof(TEntity)];

    private IMongoClient GetClient()
    {
        _mongoClient ??= new MongoClient(_settings);
        return _mongoClient;
    }
}