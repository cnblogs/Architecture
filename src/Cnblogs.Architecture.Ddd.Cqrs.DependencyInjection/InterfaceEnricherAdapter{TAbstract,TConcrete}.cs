using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;

/// <summary>
///     Adapts an <see cref="IEnricher{TAbstract}" /> to work as an <see cref="IEnricher{TConcrete}" />,
///     enabling interface-based enricher registrations.
/// </summary>
internal sealed class InterfaceEnricherAdapter<TAbstract, TConcrete>(IEnricher<TAbstract> inner) : IEnricher<TConcrete>
    where TAbstract : class
    where TConcrete : class, TAbstract
{
    public bool AllowParallel => inner.AllowParallel;

    public Task EnrichAsync(TConcrete? model, CancellationToken cancellationToken = default)
    {
        return inner.EnrichAsync(model, cancellationToken);
    }

    public Task BulkEnrichAsync(IEnumerable<TConcrete?> models, CancellationToken cancellationToken = default)
    {
        return inner.BulkEnrichAsync(models, cancellationToken);
    }
}
