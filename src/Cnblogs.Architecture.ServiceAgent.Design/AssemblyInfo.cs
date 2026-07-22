using Cnblogs.Architecture.ServiceAgent.Design;
using Microsoft.AspNetCore.Hosting;

// Auto-activates ServiceAgentExporterStartup when this assembly is listed in
// ASPNETCORE_HOSTINGSTARTUPASSEMBLIES (the cnb-arch-tool does this during a generation run).
[assembly: HostingStartup(typeof(ServiceAgentExporterStartup))]
