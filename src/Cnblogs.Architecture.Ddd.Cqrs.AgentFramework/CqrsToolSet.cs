using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     The set of <see cref="AIFunction" /> tools generated from the opted-in CQRS Commands and Queries. Resolved from DI as a singleton.
/// </summary>
public sealed class CqrsToolSet
{
    /// <summary>
    ///     Creates a tool set.
    /// </summary>
    /// <param name="tools">The generated tools, one per exposed Command/Query.</param>
    /// <param name="byRequestType">The same tools keyed by their CQRS request type, for per-agent filtering.</param>
    public CqrsToolSet(IReadOnlyList<AIFunction> tools, IReadOnlyDictionary<Type, AIFunction> byRequestType)
    {
        Tools = tools;
        ByRequestType = byRequestType;
    }

    /// <summary>
    ///     The generated tools, one per exposed Command/Query.
    /// </summary>
    public IReadOnlyList<AIFunction> Tools { get; }

    /// <summary>
    ///     The generated tools keyed by their CQRS request type, used to filter tools per agent (<c>ICqrsAgent.AllowedRequestTypes</c>).
    /// </summary>
    public IReadOnlyDictionary<Type, AIFunction> ByRequestType { get; }
}
