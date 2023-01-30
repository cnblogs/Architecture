using System.Data;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

/// <summary>
///     提供 DbConnection 的工厂接口。
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    ///     获取 DbConnection。
    /// </summary>
    /// <returns></returns>
    IDbConnection CreateDbConnection();
}