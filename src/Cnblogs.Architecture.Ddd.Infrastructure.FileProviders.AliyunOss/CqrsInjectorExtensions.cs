using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Credentials;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     Extension methods to inject Aliyun OSS provider to CQRS injector.
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     Use aliyun oss as default implementation of <see cref="IFileProvider"/> and <see cref="IFileDeliveryProvider"/>.
    /// </summary>
    /// <param name="injector"></param>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionName"></param>
    /// <returns></returns>
    public static CqrsInjector AddAliyunOssFileProviders(
        this CqrsInjector injector,
        IConfiguration configuration,
        string configurationSectionName = "ossClient")
    {
        injector.Services.Configure<AliyunOssOptions>(configuration.GetSection(configurationSectionName));
        injector.Services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AliyunOssOptions>>().Value;
            var cfg = Configuration.LoadDefault();
            cfg.CredentialsProvider = GetCredentialsProvider(options);
            cfg.ConfigureEndpoint(options);
            cfg.Region = options.Region;
            return cfg;
        });
        injector.Services.AddSingleton(sp =>
        {
            var cfg = sp.GetRequiredService<Configuration>();
            return new Client(cfg);
        });
        return injector.AddFileProvider<AliyunOssFileProvider>()
            .AddFileDeliveryProvider<AliyunOssFileDeliveryProvider>();
    }

    private static void ConfigureEndpoint(this Configuration cfg, AliyunOssOptions options)
    {
        cfg.DisableSsl = options.UseHttps == false;
        if (string.IsNullOrWhiteSpace(options.Endpoint) == false)
        {
            cfg.Endpoint = options.Endpoint.Trim();
        }
        else
        {
            cfg.UseInternalEndpoint = options.UseInternalEndpoint;
            cfg.UseAccelerateEndpoint = options.UseAccelerateEndpoint;
        }
    }

    private static ICredentialsProvider GetCredentialsProvider(AliyunOssOptions options)
    {
        ICredentialsProvider credentialsProvider;
        if (string.IsNullOrEmpty(options.AccessKeyId))
        {
            // use ecs-ram credential by default
            var credConfig = new Aliyun.Credentials.Models.Config() { Type = "ecs_ram_role", };
            var credClient = new Aliyun.Credentials.Client(credConfig);

            credentialsProvider = new CredentialsProviderFunc(() =>
            {
                // 获取临时凭证
                var credential = credClient.GetCredential();

                // 构造OSS SDK所需的凭证对象
                return new Credentials(
                    credential.AccessKeyId,
                    credential.AccessKeySecret,
                    credential.SecurityToken);
            });
        }
        else
        {
            // use STS or AK
            credentialsProvider = options.SecurityToken == null
                ? new StaticCredentialsProvider(options.AccessKeyId, options.AccessKeySecret)
                : new StaticCredentialsProvider(options.AccessKeyId, options.AccessKeySecret, options.SecurityToken);
        }

        return credentialsProvider;
    }
}
