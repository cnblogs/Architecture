namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     Indicates whether the current host startup is a service-agent generation run, driven by the
///     <c>dotnet-cnblogs-sa</c> global tool. API startup code may guard side-effecting initializers (database
///     migrations, message-bus connections, ...) behind <see cref="IsActive" /> so the design-time run stays cheap
///     and side-effect-free, mirroring the EF Core design-time convention.
/// </summary>
public static class ServiceAgentGeneration
{
    /// <summary>
    ///     The environment variable that, when set, activates a generation run. Its value is the output path of the
    ///     endpoint manifest to write.
    /// </summary>
    public const string ExportPathEnvironmentVariable = "CNBLOGS_SA_EXPORT";

    /// <summary>The environment variable used to activate this assembly's hosting startup during a generation run.</summary>
    public const string HostingStartupEnvironmentVariable = "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES";

    /// <summary>The assembly name to list under <see cref="HostingStartupEnvironmentVariable" /> to activate export.</summary>
    public const string AssemblyName = "Cnblogs.Architecture.ServiceAgent.Design";

    /// <summary>The manifest output path, or <c>null</c> when no generation run is active.</summary>
    public static string? ExportPath => Environment.GetEnvironmentVariable(ExportPathEnvironmentVariable);

    /// <summary>Whether the current run is a service-agent generation run.</summary>
    public static bool IsActive => !string.IsNullOrWhiteSpace(ExportPath);
}
