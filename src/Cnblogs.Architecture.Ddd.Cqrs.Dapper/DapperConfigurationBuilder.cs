using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper;

/// <summary>
///     Dapper 配置类。
/// </summary>
/// <typeparam name="TContext">The context type been configured.</typeparam>
public class DapperConfigurationBuilder<TContext>
    where TContext : DapperContext
{
    private readonly string _dapperContextTypeName;

    /// <summary>
    ///     创建一个 DapperConfigurationBuilder。
    /// </summary>
    /// <param name="services"><see cref="ServiceCollection"/></param>
    public DapperConfigurationBuilder(IServiceCollection services)
    {
        _dapperContextTypeName = typeof(TContext).Name;
        Services = services;
    }

    /// <summary>
    ///     使用指定的 <see cref="IDbConnectionFactory"/>。
    /// </summary>
    /// <param name="factory">工厂对象。</param>
    /// <typeparam name="TFactory">工厂类型。</typeparam>
    public void UseDbConnectionFactory<TFactory>(TFactory factory)
        where TFactory : IDbConnectionFactory
    {
        Services.Configure<DbConnectionFactoryCollection>(
            c => c.AddDbConnectionFactory(_dapperContextTypeName, factory));
    }

    /// <summary>
    ///     The underlying <see cref="IServiceCollection"/>.
    /// </summary>
    public IServiceCollection Services { get; }
}
