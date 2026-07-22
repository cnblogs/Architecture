namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>One settable property of a generated payload POCO.</summary>
public sealed class ManifestPayloadProperty
{
    /// <summary>The property name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The property CLR type.</summary>
    public ClrTypeRef ClrType { get; set; } = new();

    /// <summary>Whether the property is nullable.</summary>
    public bool IsNullable { get; set; }
}
