using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class TestClickhouseDapperContext(
    IOptions<DbConnectionFactoryCollection> dbConnectionFactoryCollection,
    ClickhouseContextOptions<TestClickhouseDapperContext> options,
    IServiceProvider serviceProvider)
    : ClickhouseDapperContext(dbConnectionFactoryCollection, options, serviceProvider)
{
    public bool ConfigureModelsCalled { get; set; }

    public IDbConnectionFactory Factory => DbConnectionFactory;

    /// <inheritdoc />
    protected override void ConfigureModels(ClickhouseModelCollectionBuilder builder)
    {
        ConfigureModelsCalled = true;
    }
}
