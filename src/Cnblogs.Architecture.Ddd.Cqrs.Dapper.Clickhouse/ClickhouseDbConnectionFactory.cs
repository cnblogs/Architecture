using System.Data;
using ClickHouse.Client;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;

/// <summary>
///     Clickhouse connection factory.
/// </summary>
/// <typeparam name="TContext">The <see cref="ClickhouseDapperContext"/> this connection factory belongs to.</typeparam>
public class ClickhouseDbConnectionFactory<TContext>(IClickHouseDataSource dataSource) : IDbConnectionFactory
    where TContext : ClickhouseDapperContext
{
    /// <inheritdoc />
    public IDbConnection CreateDbConnection()
    {
        return dataSource.CreateConnection();
    }
}
