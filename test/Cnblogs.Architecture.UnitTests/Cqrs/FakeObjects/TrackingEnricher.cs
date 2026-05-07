using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class TrackingEnricher : IEnricher<FakePostDto>
{
    public List<FakePostDto> EnrichedItems { get; } = [];

    /// <inheritdoc />
    public bool AllowParallel { get; set; }

    /// <inheritdoc />
    public Task EnrichAsync(FakePostDto model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnrichedItems.Add(model);
        return Task.CompletedTask;
    }
}

public class TrackingEnricher2 : TrackingEnricher;
