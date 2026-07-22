namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     One settable property of a request-body payload, captured so a service-agent generator can emit a POCO that
///     mirrors the command's wire shape without referencing the command type's defining assembly.
/// </summary>
public sealed class EndpointPayloadProperty
{
    /// <summary>The property name.</summary>
    public required string Name { get; init; }

    /// <summary>The property CLR type.</summary>
    public required Type ClrType { get; init; }

    /// <summary>Whether the property is nullable.</summary>
    public bool IsNullable { get; init; }
}
