using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
///     ServiceAgent errors.
/// </summary>
public class ServiceAgentError : Enumeration
{
    /// <summary>
    ///     The default error code.
    /// </summary>
    public static readonly ServiceAgentError UnknownError = new(-1, "Unknown error");

    /// <summary>
    ///     Create a service agent error.
    /// </summary>
    /// <param name="id">The error code.</param>
    /// <param name="name">The error name.</param>
    public ServiceAgentError(int id, string name)
        : base(id, name)
    {
    }
}
