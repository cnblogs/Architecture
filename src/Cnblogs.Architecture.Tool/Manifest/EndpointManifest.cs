namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class EndpointManifest
{
    public int SchemaVersion { get; set; } = 1;
    public List<ManifestGroup> Groups { get; set; } = [];
}
