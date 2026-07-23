namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     The wire shape of a command-as-body request, used to generate a standalone payload POCO so the generated client
///     need not reference the command type's assembly. Present only when the body type is the command itself.
/// </summary>
public sealed class ManifestPayloadContract
{
    /// <summary>The settable properties that make up the payload's JSON shape (possibly empty).</summary>
    public List<ManifestPayloadProperty> Properties { get; set; } = [];
}
