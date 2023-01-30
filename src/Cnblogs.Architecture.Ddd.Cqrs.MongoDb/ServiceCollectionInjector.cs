using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.MongoDb;

/// <summary>
///     向 <see cref="IServiceCollection"/> 注入 Mongodb 服务。
/// </summary>
public static class ServiceCollectionInjector
{
    /// <summary>
    ///     添加一个 MongoContext。
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="connectionString">链接字符串。</param>
    /// <param name="databaseName">数据库名称。</param>
    /// <typeparam name="TContext">MongoContext</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddMongoContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
        where TContext : MongoContext
    {
        services.Configure<MongoContextCollection>(o => o.Add<TContext>());
        services.AddHostedService<MongoConfigureService>();
        services.AddScoped<TContext>();
        services.AddSingleton<MongoContextOptions<TContext>>(
            _ => new MongoContextOptions<TContext>(connectionString, databaseName));
        return services;
    }
}