namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class ManifestGroup
{
    public string Name { get; set; } = string.Empty;
    public ClrTypeRef? ErrorType { get; set; }
    public List<ManifestEndpoint> Endpoints { get; set; } = [];
}
