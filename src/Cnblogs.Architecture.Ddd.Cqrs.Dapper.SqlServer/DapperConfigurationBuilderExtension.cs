using Cnblogs.Architecture.Ddd.Cqrs.Dapper.SqlServer;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

// ReSharper disable once CheckNamespace
namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper;

/// <summary>
///     Extension methods to configure dapper context.
/// </summary>
public static class DapperConfigurationBuilderExtension
{
    /// <summary>
    ///     Configure <see cref="DapperContext"/> to use sql server as underlying database.
    /// </summary>
    /// <param name="builder"><see cref="DapperConfigurationBuilder{TContext}"/></param>
    /// <param name="connectionString">The connection string for sql server.</param>
    /// <typeparam name="TContext">The type of context been configured.</typeparam>
    public static void UseSqlServer<TContext>(this DapperConfigurationBuilder<TContext> builder, string connectionString)
        where TContext : DapperContext
    {
        builder.UseDbConnectionFactory(new SqlServerDbConnectionFactory(connectionString));
    }
}
