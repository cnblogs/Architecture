using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class TrackingEnricher : IEnricher<FakePostDto>
{
    public List<FakePostDto> EnrichedItems { get; } = [];

    /// <inheritdoc />
    public bool AllowParallel { get; set; }

    /// <inheritdoc />
    public Task EnrichAsync(FakePostDto? model, CancellationToken cancellationToken = default)
    {
        if (model is not null)
        {
            EnrichedItems.Add(model);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task BulkEnrichAsync(IEnumerable<FakePostDto?> models, CancellationToken cancellationToken = default)
    {
        EnrichedItems.AddRange(models.Where(m => m is not null).Cast<FakePostDto>());
        return Task.CompletedTask;
    }
}