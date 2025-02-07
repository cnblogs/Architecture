using System.Data;
using ClickHouse.Client;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;

/// <summary>
///     Clickhouse connection factory.
/// </summary>
public class ClickhouseDbConnectionFactory(IClickHouseDataSource dataSource) : IDbConnectionFactory
{
    /// <inheritdoc />
    public IDbConnection CreateDbConnection()
    {
        return dataSource.CreateConnection();
    }
}
