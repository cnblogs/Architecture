namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Base enricher implementation for <see cref="IEnricher{T}"/>
/// </summary>
/// <typeparam name="T">The type of items to be enriched</typeparam>
public abstract class Enricher<T> : IEnricher<T>
    where T : class
{
    /// <inheritdoc />
    public abstract Task EnrichAsync(T? model, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual async Task BulkEnrichAsync(IEnumerable<T?> models, CancellationToken cancellationToken = default)
    {
        foreach (var model in models)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await EnrichAsync(model, cancellationToken);
        }
    }

    /// <inheritdoc />
    public abstract bool AllowParallel { get; }
}
