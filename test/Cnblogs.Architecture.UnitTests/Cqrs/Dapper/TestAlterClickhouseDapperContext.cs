using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class TestAlterClickhouseDapperContext(
    IOptions<DbConnectionFactoryCollection> dbConnectionFactoryCollection,
    ClickhouseContextOptions<TestAlterClickhouseDapperContext> options,
    IServiceProvider serviceProvider)
    : ClickhouseDapperContext(dbConnectionFactoryCollection, options, serviceProvider)
{
    /// <inheritdoc />
    protected override void ConfigureModels(ClickhouseModelCollectionBuilder builder)
    {
        // ignore
    }

    public IDbConnectionFactory Factory => DbConnectionFactory;
}
