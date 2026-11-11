namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Options for exposing opted-in CQRS Commands and Queries as MCP tools.
/// </summary>
public sealed class CqrsMcpOptions
{
    /// <summary>
    ///     An optional predicate on the Command/Query type. Only types for which it returns <see langword="true" /> are exposed over
    ///     MCP; when <see langword="null" /> (default), every tool built by <c>AddAgentFramework()</c> is exposed. Use it to expose a
    ///     subset of the agent tool set, for example only the Queries of a read-only MCP endpoint.
    /// </summary>
    public Func<Type, bool>? RequestFilter { get; set; }

    /// <summary>
    ///     Whether Queries are annotated with the read-only MCP tool hint (<c>readOnlyHint</c>), so clients can skip confirmation for
    ///     non-mutating tools. Default is <see langword="true" />.
    /// </summary>
    public bool AnnotateQueriesAsReadOnly { get; set; } = true;
}
