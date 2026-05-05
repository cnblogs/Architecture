using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Enrichers;

public class EnrichableModelEnricher : Enricher<IEnrichableModel>
{
    /// <inheritdoc />
    public override bool AllowParallel => false;

    /// <inheritdoc />
    public override Task EnrichAsync(IEnrichableModel? model, CancellationToken cancellationToken = default)
    {
        model?.Enriched = true;
        return Task.CompletedTask;
    }
}
