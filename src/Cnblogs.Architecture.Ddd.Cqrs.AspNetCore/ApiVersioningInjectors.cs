using Asp.Versioning;
using Asp.Versioning.Conventions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Api Versioning 注入扩展
/// </summary>
public static class ApiVersioningInjectors
{
    /// <summary>
    /// 添加 API Versioning，默认使用 <see cref="VersionByNamespaceConvention"/>
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