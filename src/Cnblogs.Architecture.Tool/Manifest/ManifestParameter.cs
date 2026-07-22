namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class ManifestParameter
{
    public string Name { get; set; } = string.Empty;
    public ParameterSource Source { get; set; }
    public ClrTypeRef ClrType { get; set; } = new();
    public bool IsNullable { get; set; }
    public bool HasDefaultValue { get; set; }
    public string? DefaultValueLiteral { get; set; }
    public string? RouteToken { get; set; }
}
