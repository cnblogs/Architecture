using Cnblogs.Architecture.Ddd.Cqrs.Dapper;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     ServiceCollection 注入类。
/// </summary>
public static class ServiceCollectionInjector
{
    /// <summary>
    ///     添加一个 <see cref="DapperContext"/>。
    /// </summary>
    /// <param name="services"><see cref="ServiceCollection"/>。</param>
    /// <typeparam name="TContext"><see cref="DapperContext"/> 的实现类型。</typeparam>
    /// <returns></returns>
    public static DapperConfigurationBuilder AddDapperContext<TContext>(this IServiceCollection services)
        where TContext : DapperContext
    {
        services.AddScoped<TContext>();
        return new DapperConfigurationBuilder(typeof(TContext).Name, services);
    }
}