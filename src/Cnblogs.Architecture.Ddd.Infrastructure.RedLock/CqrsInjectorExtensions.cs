using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Cnblogs.Architecture.Ddd.Infrastructure.RedLock;

/// <summary>
///     用于向 <see cref="CqrsInjector" /> 注入 RedLock 的扩展方法。
/// </summary>
public static class CqrsInjectorExtensions
{
    /// <summary>
    ///     添加 RedLock 的依赖注入。
    /// </summary>
    /// <param name="injector"><see cref="CqrsInjector" />。</param>
    /// <param name="configuration"><see cref="IConfiguration" />。</param>
    /// <param name="section">配置名称。</param>
    /// <returns></returns>
    public static CqrsInjector AddRedLockDistributionLock(
        this CqrsInjector injector,
        IConfiguration configuration,
        string section = "RedLock")
    {
        injector.Services.Configure<RedLockOptions>(configuration.GetSection(section));
        injector.Services.AddSingleton(
            sp =>
            {
                var option = sp.GetRequiredService<IOptions<RedLockOptions>>().Value;
                return RedLockFactory.Create(
                    new List<RedLockMultiplexer> { ConnectionMultiplexer.Connect(option.GetConnectionString()) });
            });
        injector.AddDistributionLock<RedLockDistributionLockProvider>();
        return injector;
    }
}