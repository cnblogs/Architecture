namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>The top-level manifest written by the exporter and consumed by the code generator.</summary>
public sealed class EndpointManifest
{
    /// <summary>The manifest schema version. Bumped on incompatible changes; consumers should reject unknown majors.</summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>The groups, each of which becomes one generated <c>IXxxService</c> / <c>XxxService</c> pair.</summary>
    public List<ManifestGroup> Groups { get; set; } = [];
}
