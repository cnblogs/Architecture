using ClickHouse.Client;
using Cnblogs.Architecture.Ddd.Cqrs.Dapper;
using Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class ClickhouseDependencyInjectorTests
{
    [Fact]
    public void UseClickhouse_InjectSingle_Injected()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDapperContext<TestClickhouseDapperContext>().UseClickhouse("HOST=clickhouse");
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<TestClickhouseDapperContext>();
        var clickhouseDataSource =
            serviceProvider.GetRequiredKeyedService<IClickHouseDataSource>(nameof(TestClickhouseDapperContext));

        // Assert
        Assert.NotNull(dbContext);
        Assert.NotNull(clickhouseDataSource);
    }

    [Fact]
    public void UseClickhouse_InjectMultiple_Injected()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDapperContext<TestClickhouseDapperContext>().UseClickhouse("HOST=clickhouse");
        services.AddDapperContext<TestAlterClickhouseDapperContext>().UseClickhouse("HOST=alterclickhouse");
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<TestClickhouseDapperContext>();
        var alterContext = serviceProvider.GetRequiredService<TestAlterClickhouseDapperContext>();
        var dbFactory = dbContext.Factory;
        var alterFactory = alterContext.Factory;

        // Assert
        Assert.True(dbFactory is ClickhouseDbConnectionFactory<TestClickhouseDapperContext>);
        Assert.True(alterFactory is ClickhouseDbConnectionFactory<TestAlterClickhouseDapperContext>);
    }
}
