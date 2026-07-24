using System.Text.Json;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Options for generating Microsoft.Extensions.AI tools from CQRS Commands and Queries.
/// </summary>
public sealed class CqrsAgentOptions
{
    /// <summary>
    ///     How a Command/Query type becomes a tool. Default is <see cref="ToolDiscovery.Marked" /> (only opted-in types are exposed).
    /// </summary>
    public ToolDiscovery Discovery { get; set; } = ToolDiscovery.Marked;

    /// <summary>
    ///     Drops CQRS infrastructure parameters (<c>ValidateOnly</c>, <c>OrderByString</c>, <c>PagingParams</c>) from each tool's
    ///     JSON schema and binds them server-side. Default is <see langword="true" />.
    /// </summary>
    public bool HideCqrsInfrastructureParameters { get; set; } = true;

    /// <summary>
    ///     The maximum <c>pageSize</c> the model may request for an <c>IPageableQuery&lt;TElement&gt;</c> tool, to bound token cost. Default is 20.
    /// </summary>
    public int MaxPageSize { get; set; } = 20;

    /// <summary>
    ///     The <see cref="JsonSerializerOptions" /> used both for schema generation and for (de)serializing tool arguments and results.
    ///     When <see langword="null" />, <c>AIJsonUtilities.DefaultOptions</c> is used.
    /// </summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }
}
