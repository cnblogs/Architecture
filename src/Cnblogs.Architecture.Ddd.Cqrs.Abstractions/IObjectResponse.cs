namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents response that contains object result.
/// </summary>
public interface IObjectResponse
{
    /// <summary>
    ///     Get object result.
    /// </summary>
    /// <returns>The resulting object.</returns>
    public object? GetResult();
}