using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.SqlServer;

/// <summary>
///     用于配置 Dapper Configuration 的扩展方法。
/// </summary>
public static class DapperConfigurationBuilderExtension
{
    /// <summary>
    ///     使用 SqlServer 配置 <see cref="DapperContext"/>
    /// </summary>
    /// <param name="builder"><see cref="DapperConfigurationBuilder"/></param>
    /// <param name="connectionString">连接字符串。</param>
    public static void UseSqlServer(this DapperConfigurationBuilder builder, string connectionString)
    {
        builder.UseDbConnectionFactory(new SqlServerDbConnectionFactory(connectionString));
    }
}