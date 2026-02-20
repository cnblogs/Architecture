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
        return services.AddCqrs(null, assemblies);
    }

    /// <summary>
    ///     添加 Cqrs 支持。
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />。</param>
    /// <param name="configuration">The action used to configure the MediatRServiceConfiguration.</param>
    /// <param name="assemblies">命令/查询所在的程序集。</param>
    /// <returns></returns>
    public static CqrsInjector AddCqrs(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configuration,
        params Assembly[] assemblies)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        if (assemblies.Length == 0)
        {
            // mediator needs at least one assembly to inject from
            assemblies = [typeof(CqrsInjector).Assembly];
        }

        configuration ??= cfg => cfg.RegisterGenericHandlers = true;
        services.AddMediatR(cfg =>
        {
            configuration?.Invoke(cfg);
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        return new CqrsInjector(services);
    }
}
