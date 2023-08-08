using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     Extension methods to inject Aliyun OSS provider to CQRS injector.
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     Use aliyun oss as default implementation of <see cref="IFileProvider"/>.
    /// </summary>
    /// <param name="injector"></param>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionName"></param>
    /// <returns></returns>
    public static CqrsInjector UseAliyunOssFileProvider(
        this CqrsInjector injector,
        IConfiguration configuration,
        string configurationSectionName = "ossClient")
    {
        injector.Services.AddOssClient(configuration, configurationSectionName);
        injector.Services.Configure<AliyunOssOptions>(configuration.GetSection(configurationSectionName));
        return injector.AddFileProvider<AliyunOssFileProvider>();
    }
}
