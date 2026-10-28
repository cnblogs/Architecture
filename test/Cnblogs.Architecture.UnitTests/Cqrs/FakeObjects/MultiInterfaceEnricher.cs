using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class MultiInterfaceEnricher : IEnricher<FakePostDto>, IEnricher<FakeCommentDto>
{
    public List<FakePostDto> EnrichedPosts { get; } = [];

    public List<FakeCommentDto> EnrichedComments { get; } = [];

    /// <inheritdoc />
    public bool AllowParallel => false;

    public Task EnrichAsync(FakePostDto model, CancellationToken cancellationToken = default)
    {
        EnrichedPosts.Add(model);
        return Task.CompletedTask;
    }

    public Task EnrichAsync(FakeCommentDto model, CancellationToken cancellationToken = default)
    {
        EnrichedComments.Add(model);
        return Task.CompletedTask;
    }
}
