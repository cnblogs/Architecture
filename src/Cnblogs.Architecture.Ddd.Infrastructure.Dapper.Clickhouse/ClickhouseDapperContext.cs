using ClickHouse.Client.Copy;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

/// <summary>
///     DapperContext that specialized for clickhouse.
/// </summary>
public abstract class ClickhouseDapperContext : DapperContext
{
    private readonly ClickhouseContextOptions _options;

    /// <summary>
    ///     Create a <see cref="ClickhouseDapperContext"/>.
    /// </summary>
    /// <param name="dbConnectionFactoryCollection">The underlying <see cref="IDbConnectionFactory"/> collection.</param>
    /// <param name="options">The options used for this context.</param>
    /// <param name="serviceProvider">The service provider to use.</param>
    protected ClickhouseDapperContext(
        IOptions<DbConnectionFactoryCollection> dbConnectionFactoryCollection,
        ClickhouseContextOptions options,
        IServiceProvider serviceProvider)
        : base(dbConnectionFactoryCollection, serviceProvider)
    {
        _options = options;
    }

    /// <summary>
    ///     Init context, register models, etc.
    /// </summary>
    public void Init()
    {
        var builder = new ClickhouseModelCollectionBuilder();
        ConfigureModels(builder);
        builder.Build(_options);
    }

    /// <summary>
    ///     Configure models that related to this context.
    /// </summary>
    /// <param name="builder"><see cref="ClickhouseModelCollectionBuilder"/>.</param>
    protected abstract void ConfigureModels(ClickhouseModelCollectionBuilder builder);

    /// <summary>
    ///     Bulk write entities to clickhouse.
    /// </summary>
    /// <param name="entities">The entity to be written.</param>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <exception cref="InvalidOperationException">Throw when <typeparamref name="T"/> is not registered.</exception>
    public async Task BulkWriteAsync<T>(IEnumerable<T> entities)
        where T : class
    {
        var configuration = _options.GetConfiguration<T>();
        if (configuration is null)
        {
            throw new InvalidOperationException(
                $"The model type {typeof(T).Name} is not registered, make sure you have called builder.Entity<T>() at ConfigureModels()");
        }

        using var bulkCopyInterface = new ClickHouseBulkCopy(_options.ConnectionString)
        {
            DestinationTableName = configuration.TableName,
            ColumnNames = configuration.ColumnNames
        };

        var objs = entities.Select(x => configuration.ToObjectArray(x));
        await bulkCopyInterface.InitAsync();
        await bulkCopyInterface.WriteToServerAsync(objs);
    }
}
