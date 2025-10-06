using System.Reflection;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using MediatR;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

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
        if (assemblies.Length == 0)
        {
            // mediator needs at least one assembly to inject from
            assemblies = [typeof(CqrsInjector).Assembly];
        }

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            cfg.RegisterGenericHandlers = true;
        });

        return new CqrsInjector(services);
    }
}
