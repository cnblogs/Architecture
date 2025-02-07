using Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper;

/// <summary>
///     Extension methods for inject clickhouse to dapper context.
/// </summary>
public static class DapperConfigurationBuilderExtension
{
    /// <summary>
    ///     Use clickhouse as the underlying database.
    /// </summary>
    /// <param name="builder"><see cref="DapperConfigurationBuilder{TContext}"/>.</param>
    /// <param name="connectionString">The connection string for clickhouse.</param>
    /// <typeparam name="TContext">The context type been used.</typeparam>
    public static void UseClickhouse<TContext>(
        this DapperConfigurationBuilder<TContext> builder,
        string connectionString)
        where TContext : ClickhouseDapperContext
    {
        builder.UseDbConnectionFactory<ClickhouseDbConnectionFactory>();
        builder.Services.AddClickHouseDataSource(connectionString);
        builder.Services.AddSingleton(new ClickhouseContextOptions<TContext>(connectionString));
        builder.Services.Configure<ClickhouseContextCollection>(x => x.Add<TContext>());
        builder.Services.AddHostedService<ClickhouseInitializeHostedService>();
    }
}
