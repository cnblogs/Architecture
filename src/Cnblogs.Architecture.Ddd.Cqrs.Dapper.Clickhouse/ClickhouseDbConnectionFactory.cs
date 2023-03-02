using System.Data;
using ClickHouse.Client.ADO;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;

/// <summary>
///     Clickhouse connection factory.
/// </summary>
public class ClickhouseDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    ///     Create a clickhouse connection factory.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public ClickhouseDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateDbConnection()
    {
        return new ClickHouseConnection(_connectionString);
    }
}
