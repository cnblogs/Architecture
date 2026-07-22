namespace Cnblogs.Architecture.UnitTests.Cqrs;

/// <summary>
///     A test collection for tests that mutate process-global state (e.g. environment variables) and so cannot run
///     in parallel with anything else. xUnit runs collections in parallel by default; assigning every such test to
///     this collection serializes them.
/// </summary>
[CollectionDefinition(Name)]
public class SerialCollection
{
    public const string Name = "Service-Agent Serial";
}
