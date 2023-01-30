using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
/// 注入 Serilog 的扩展方法。
/// </summary>
public static class SerilogInjector
{
    /// <summary>
    /// 添加 Serilog
    /// </summary>
    /// <param name="builder"><see cref="WebApplicationBuilder"/></param>
    /// <returns></returns>
    public static IHostBuilder UseCnblogsSerilog(this WebApplicationBuilder builder)
    {
        return builder.Host.UseCnblogsSerilog();
    }

    /// <summary>
    /// 添加 Serilog
    /// </summary>
    /// <param name="host"><see cref="IHostBuilder"/></param>
    /// <returns></returns>
    public static IHostBuilder UseCnblogsSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((ctx, conf) => conf.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext());
    }
}