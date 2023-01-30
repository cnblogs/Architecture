using System.Reflection;

using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

/// <summary>
///     依赖注入扩展方法。
/// </summary>
public static class ServiceCollectionInjector
{
    /// <summary>
    ///     添加 Cqrs 支持。
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />。</param>
    /// <param name="assemblies">命令/查询所在的程序集。</param>
    /// <returns></returns>
    public static CqrsInjector AddCqrs(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddMediatR(assemblies);
        return new CqrsInjector(services);
    }
}