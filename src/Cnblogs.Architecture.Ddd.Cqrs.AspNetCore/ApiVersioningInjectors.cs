using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
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
    /// <param name="versioningSetup">Versioning Setup.</param>
    /// <param name="mvcApiVersioningSetup">Setups in MVC api versioning.</param>
    /// <param name="apiExplorerSetup">Setups for ApiExplorer.</param>
    /// <returns></returns>
    public static IApiVersioningBuilder AddCnblogsApiVersioning(
        this IServiceCollection services,
        Action<ApiVersioningOptions>? versioningSetup = null,
        Action<MvcApiVersioningOptions>? mvcApiVersioningSetup = null,
        Action<ApiExplorerOptions>? apiExplorerSetup = null)
    {
        services.AddEndpointsApiExplorer();
        return services.AddApiVersioning(o =>
            {
                versioningSetup?.Invoke(o);
            })
            .AddMvc(o =>
            {
                o.Conventions.Add(new VersionByNamespaceConvention());
                mvcApiVersioningSetup?.Invoke(o);
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
                apiExplorerSetup?.Invoke(o);
            });
    }
}
