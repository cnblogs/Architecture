using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Enrichers;

public class EnrichableModelEnricher : IEnricher<IEnrichableModel>
{
    /// <inheritdoc />
    public bool AllowParallel => false;

    /// <inheritdoc />
    public Task EnrichAsync(IEnrichableModel model, CancellationToken cancellationToken = default)
    {
        model.Enriched = true;
        return Task.CompletedTask;
    }
}
