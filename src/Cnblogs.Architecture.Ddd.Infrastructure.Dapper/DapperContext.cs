using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

/// <summary>
///     用于与数据库交互的上下文实例，使用方式类似于 DbContext
/// </summary>
public abstract class DapperContext
{
    /// <summary>
    ///     创建一个 DapperContext。
    /// </summary>
    /// <param name="dbConnectionFactoryCollection">数据库连接工厂集合。</param>
    /// <param name="sp">The service provider to get connection factory</param>
    protected DapperContext(IOptions<DbConnectionFactoryCollection> dbConnectionFactoryCollection, IServiceProvider sp)
    {
        var type = dbConnectionFactoryCollection.Value.GetFactory(GetType().Name);
        DbConnectionFactory = sp.GetRequiredService(type) as IDbConnectionFactory
                              ?? throw new InvalidOperationException(
                                  $"No DbConnectionFactory(type: {type.Name}) configured.");
    }

    /// <summary>
    ///     数据库连接工厂。
    /// </summary>
    protected IDbConnectionFactory DbConnectionFactory { get; }

    /// <summary>
    ///     创建一个数据库连接。
    /// </summary>
    /// <returns></returns>
    public IDbConnection CreateDbConnection() => DbConnectionFactory.CreateDbConnection();
}
