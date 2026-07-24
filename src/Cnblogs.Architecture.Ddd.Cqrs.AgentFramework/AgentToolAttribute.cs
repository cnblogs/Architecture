namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Opts a Command or Query in (or out) of agent-tool exposure, and overrides generated tool metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AgentToolAttribute : Attribute
{
    /// <summary>
    ///     Whether the type is exposed as a tool. Use <see cref="ToolExposure.Exclude" /> to opt out under
    ///     <see cref="ToolDiscovery.AllValidRequests" />. Default is <see cref="ToolExposure.Include" />.
    /// </summary>
    public ToolExposure Exposure { get; set; } = ToolExposure.Include;

    /// <summary>
    ///     Overrides the tool name. Default is the record type name (e.g. <c>CreateCommand</c>).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Overrides the tool description. Default is the type's XML doc <c>&lt;summary&gt;</c>.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     When <see langword="true" />, exposes the CQRS <c>ValidateOnly</c> parameter to the model so it can dry-run a Command
    ///     (run validation without executing). Default is <see langword="false" />; the parameter is hidden and always <see langword="false" />.
    /// </summary>
    public bool AllowValidateOnly { get; set; }
}
