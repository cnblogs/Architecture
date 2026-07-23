namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class PayloadProperty
{
    public string Name { get; set; } = string.Empty;
    public ClrTypeRef ClrType { get; set; } = new();
    public bool IsNullable { get; set; }
}
