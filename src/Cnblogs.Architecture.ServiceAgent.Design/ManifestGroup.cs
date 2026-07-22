namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>A group of endpoints that share one error type and become one generated service agent.</summary>
public sealed class ManifestGroup
{
    /// <summary>The group name (becomes the suffix of <c>IXxxService</c>).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The error type (<c>TError</c>) for the group's <c>CqrsServiceAgent&lt;TError&gt;</c> base.</summary>
    public ClrTypeRef? ErrorType { get; set; }

    /// <summary>The endpoints in the group.</summary>
    public List<ManifestEndpoint> Endpoints { get; set; } = [];
}
