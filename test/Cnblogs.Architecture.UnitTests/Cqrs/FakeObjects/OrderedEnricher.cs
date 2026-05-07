using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public abstract class OrderedEnricher : IEnricher<FakePostDto>
{
    public List<string> ExecutionLog { get; set; } = [];
    public bool AllowParallel { get; set; }

    public Task EnrichAsync(FakePostDto model, CancellationToken cancellationToken)
    {
        ExecutionLog.Add(GetType().Name);
        return Task.CompletedTask;
    }

    public Task BulkEnrichAsync(IEnumerable<FakePostDto> models, CancellationToken cancellationToken)
    {
        ExecutionLog.Add(GetType().Name);
        return Task.CompletedTask;
    }
}

public class EnricherA : OrderedEnricher;

[EnrichAfter(typeof(EnricherA))]
public class EnricherB : OrderedEnricher;

[EnrichAfter(typeof(EnricherA), typeof(EnricherB))]
public class EnricherC : OrderedEnricher;

[EnrichAfter(typeof(CircularB))]
internal class CircularA : OrderedEnricher;

[EnrichAfter(typeof(CircularA))]
internal class CircularB : OrderedEnricher;