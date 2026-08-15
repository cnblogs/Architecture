using Cnblogs.Architecture.Tool.Generation;

namespace Cnblogs.Architecture.Tool.Manifest;

internal sealed class ManifestGroup
{
    public string Name { get; set; } = string.Empty;
    public ClrTypeRef? ErrorType { get; set; }
    public List<ManifestEndpoint> Endpoints { get; set; } = [];

    /// <summary>
    ///     The API version this group's <c>{version:apiVersion}</c> route tokens are stamped with. Set by
    ///     <see cref="ServiceAgentEmitter" /> from the endpoints' declared versions; <c>null</c> falls back to the
    ///     emitter-level <see cref="ServiceAgentEmitter.ApiVersion" /> default. Not serialized — the manifest carries
    ///     versions per endpoint (<see cref="ManifestEndpoint.ApiVersions" />), the group value is derived.
    /// </summary>
    public string? ApiVersion { get; set; }
}
