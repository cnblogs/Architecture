namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class ClrTypeRef
{
    public string Namespace { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsArray { get; set; }
    public int ArrayRank { get; set; }
    public ClrTypeRef[] GenericArguments { get; set; } = [];
}
