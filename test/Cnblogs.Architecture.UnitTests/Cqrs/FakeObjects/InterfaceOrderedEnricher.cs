using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public abstract class InterfaceOrderedEnricher : IEnricher<IFakeEnrichable>
{
    public List<string> ExecutionLog { get; set; } = [];
    public bool AllowParallel { get; set; }

    public Task EnrichAsync(IFakeEnrichable model, CancellationToken cancellationToken)
    {
        ExecutionLog.Add(GetType().Name);
        return Task.CompletedTask;
    }
}

public class InterfaceEnricherA : InterfaceOrderedEnricher;

[EnrichAfter(typeof(InterfaceEnricherA))]
public class InterfaceEnricherB : InterfaceOrderedEnricher;
