namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

/// <summary>
///     The options for clickhouse context.
/// </summary>
/// <typeparam name="TContext">The type of <see cref="ClickhouseDapperContext"/> been configured.</typeparam>
public class ClickhouseContextOptions<TContext> : ClickhouseContextOptions
    where TContext : ClickhouseDapperContext
{
    /// <summary>
    ///     Create a <see cref="ClickhouseContextOptions{TContext}"/> with given connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for clickhouse.</param>
    public ClickhouseContextOptions(string connectionString)
        : base(connectionString)
    {
    }
}

/// <summary>
///     The options for <see cref="ClickhouseDapperContext"/>.
/// </summary>
public class ClickhouseContextOptions
{
    private readonly Dictionary<Type, ClickhouseEntityConfiguration> _entityConfigurations = new();

    internal ClickhouseContextOptions(string connectionString)
    {
        ConnectionString = connectionString;
    }

    internal string ConnectionString { get; }

    internal void Add(Type type, ClickhouseEntityConfiguration configuration)
    {
        _entityConfigurations.Add(type, configuration);
    }

    internal ClickhouseEntityConfiguration? GetConfiguration<T>()
    {
        return _entityConfigurations.GetValueOrDefault(typeof(T));
    }
}
