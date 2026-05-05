namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents an enricher for certain type.
/// </summary>
/// <typeparam name="T">Typeof the model to be enriched.</typeparam>
public interface IEnricher<T> : IEnricher
    where T : class
{
    /// <summary>
    ///     Enrich a single model.
    /// </summary>
    /// <param name="model">The model to be enriched.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns></returns>
    Task EnrichAsync(T? model, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Enrich a batch of models.
    /// </summary>
    /// <param name="models">The models to be enriched.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns></returns>
    Task BulkEnrichAsync(IEnumerable<T?> models, CancellationToken cancellationToken = default);
}
