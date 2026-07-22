namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class ManifestEndpoint
{
    public string HttpMethod { get; set; } = string.Empty;
    public List<string> HttpMethods { get; set; } = [];
    public string Route { get; set; } = string.Empty;
    public bool IsQuery { get; set; }
    public ResponseShape ResponseShape { get; set; }
    public ClrTypeRef? ResponseType { get; set; }
    public ClrTypeRef? PayloadType { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public List<ManifestParameter> Parameters { get; set; } = [];
    public List<string> NullableRouteParameters { get; set; } = [];
    public bool EnableHead { get; set; }
}
