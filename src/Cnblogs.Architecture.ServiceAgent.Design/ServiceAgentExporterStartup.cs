using Microsoft.AspNetCore.Hosting;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     Hosting startup that registers the endpoint exporter. It is auto-activated when the
///     <c>dotnet-cnblogs-sa</c> global tool lists this assembly under
///     <c>ASPNETCORE_HOSTINGSTARTUPASSEMBLIES</c>; the registered hosted service is a no-op unless a generation run
///     is active.
/// </summary>
public sealed class ServiceAgentExporterStartup : IHostingStartup
{
    /// <inheritdoc />
    public void Configure(IWebHostBuilder builder)
    {
        // Route through AddServiceAgentExporter so registration stays idempotent if the host also calls it.
        builder.ConfigureServices(services => services.AddServiceAgentExporter());
    }
}
