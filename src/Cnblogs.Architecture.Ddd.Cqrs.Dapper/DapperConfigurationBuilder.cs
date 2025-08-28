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
        where TFactory : class, IDbConnectionFactory
    {
        Services.AddSingleton(factory);
        Services.Configure<DbConnectionFactoryCollection>(
            c => c.AddDbConnectionFactory(_dapperContextTypeName, typeof(TFactory)));
    }

    /// <summary>
    ///     Add <typeparamref name="TFactory"/> by <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="implementationFactory">The object initializer.</param>
    /// <typeparam name="TFactory">The type of the factory.</typeparam>
    public void UseDbConnectionFactory<TFactory>(Func<IServiceProvider, TFactory> implementationFactory)
        where TFactory : class
    {
        Services.AddSingleton(implementationFactory);
        Services.Configure<DbConnectionFactoryCollection>(
            c => c.AddDbConnectionFactory(_dapperContextTypeName, typeof(TFactory)));
    }

    /// <summary>
    ///     Add <typeparamref name="TFactory"/> as <see cref="IDbConnectionFactory"/> and get instance from DI when used.
    /// </summary>
    /// <typeparam name="TFactory">The factory type.</typeparam>
    public void UseDbConnectionFactory<TFactory>()
        where TFactory : class, IDbConnectionFactory
    {
        Services.AddSingleton<TFactory>();
        Services.Configure<DbConnectionFactoryCollection>(
            c => c.AddDbConnectionFactory(_dapperContextTypeName, typeof(TFactory)));
    }

    /// <summary>
    ///     The underlying <see cref="IServiceCollection"/>.
    /// </summary>
    public IServiceCollection Services { get; }
}
