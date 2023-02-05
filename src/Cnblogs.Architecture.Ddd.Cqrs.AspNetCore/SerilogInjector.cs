using Microsoft.AspNetCore.Builder;
using Serilog;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

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