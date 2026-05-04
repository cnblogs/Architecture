namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Mark a request as enrichable.
/// </summary>
public interface IEnrichableRequest
{
    /// <summary>
    ///     Set true if you want to disable enricher in runtime.
    /// </summary>
    bool IsEnrichSkipped()
    {
        return false;
    }
}
