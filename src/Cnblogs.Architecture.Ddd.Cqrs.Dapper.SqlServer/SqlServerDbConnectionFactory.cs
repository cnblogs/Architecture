using System.Data;

using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

using Microsoft.Data.SqlClient;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.SqlServer;

/// <summary>
///     SqlServer 数据库连接工厂。
/// </summary>
public class SqlServerDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    ///     创建一个 SqlServer 数据库连接工厂。
    /// </summary>
    /// <param name="connectionString">数据库连接字符串。</param>
    public SqlServerDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateDbConnection()
    {
        return new SqlConnection(_connectionString);
    }
}