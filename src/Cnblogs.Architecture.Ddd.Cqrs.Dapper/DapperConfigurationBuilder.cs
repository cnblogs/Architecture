using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper;

/// <summary>
///     Dapper 配置类。
/// </summary>
public class DapperConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private readonly string _dapperContextTypeName;

    /// <summary>
    ///     创建一个 DapperConfigurationBuilder。
    /// </summary>
    /// <param name="dapperContextTypeName">正在配置的 DapperContext 名称。</param>
    /// <param name="services"><see cref="ServiceCollection"/></param>
    public DapperConfigurationBuilder(string dapperContextTypeName, IServiceCollection services)
    {
        _dapperContextTypeName = dapperContextTypeName;
        _services = services;
    }

    /// <summary>
    ///     使用指定的 <see cref="IDbConnectionFactory"/>。
    /// </summary>
    /// <param name="factory">工厂对象。</param>
    /// <typeparam name="TFactory">工厂类型。</typeparam>
    public void UseDbConnectionFactory<TFactory>(TFactory factory)
        where TFactory : IDbConnectionFactory
    {
        _services.Configure<DbConnectionFactoryCollection>(
            c => c.AddDbConnectionFactory(_dapperContextTypeName, factory));
    }
}