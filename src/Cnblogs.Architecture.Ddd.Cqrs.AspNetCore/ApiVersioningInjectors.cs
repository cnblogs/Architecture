using Asp.Versioning;
using Asp.Versioning.Conventions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extension methods to inject api versioning.
/// </summary>
public static class ApiVersioningInjectors
{
    /// <summary>
    /// Add API Versioning, use <see cref="VersionByNamespaceConvention"/> by default.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <returns></returns>
    public static IApiVersioningBuilder AddCnblogsApiVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        return services.AddApiVersioning(o => o.ReportApiVersions = true)
            .AddMvc(o => o.Conventions.Add(new VersionByNamespaceConvention()))
            .AddApiExplorer(
                o =>
                {
                    o.GroupNameFormat = "'v'VVV";
                    o.SubstituteApiVersionInUrl = true;
                });
    }
}
