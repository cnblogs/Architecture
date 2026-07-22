using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     Extension methods for registering the service-agent endpoint exporter.
/// </summary>
public static class ServiceAgentExporterExtensions
{
    /// <summary>
    ///     Register the endpoint exporter hosted service explicitly. This is an alternative to the hosting-startup
    ///     activation for projects that prefer explicit registration. The registration is idempotent, so it is safe
    ///     to call this even when the hosting startup also activates (e.g. during a tool-driven run). The hosted
    ///     service is a no-op unless a generation run is active (<see cref="ServiceAgentGeneration.IsActive" />).
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddServiceAgentExporter(this IServiceCollection services)
    {
        if (services.Any(IsExporterRegistered))
        {
            return services;
        }

        services.AddHostedService<ServiceAgentExporterHostedService>();
        return services;
    }

    private static bool IsExporterRegistered(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationType == typeof(ServiceAgentExporterHostedService)
            || descriptor.ImplementationInstance is ServiceAgentExporterHostedService;
    }
}
