using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Enrichers;

[EnrichAfter(typeof(EnrichableModelEnricher))]
public class RunAfterEnricher : IEnricher<IEnrichableModel>
{
    /// <inheritdoc />
    public bool AllowParallel => true;

    /// <inheritdoc />
    public async Task EnrichAsync(IEnrichableModel model, CancellationToken cancellationToken)
    {
        if (model.Enriched == false)
        {
            throw new InvalidOperationException($"This should be called after {nameof(EnrichableModelEnricher)}");
        }

        model.EnrichedAfter = true;
    }
}
