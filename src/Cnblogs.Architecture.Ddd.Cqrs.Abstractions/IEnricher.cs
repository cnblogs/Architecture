namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Enricher contract.
/// </summary>
public interface IEnricher
{
    /// <summary>
    ///     Allow parallel call.
    /// </summary>
    bool AllowParallel { get; }
}
