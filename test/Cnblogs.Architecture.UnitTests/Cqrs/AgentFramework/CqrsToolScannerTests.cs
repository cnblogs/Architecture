using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsToolScannerTests
{
    private static readonly XmlDocumentationProvider XmlDoc = new();

    [Fact]
    public void ScanTypes_MarkedDiscovery_IncludesTaggedCommand()
    {
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentCreateCommand)], new CqrsAgentOptions(), XmlDoc);

        Assert.Contains(descriptors, d => d.RequestType == typeof(AgentCreateCommand));
    }

    [Fact]
    public void ScanTypes_MarkedDiscovery_IncludesMarkerInterfaceCommand()
    {
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentMarkerCommand)], new CqrsAgentOptions(), XmlDoc);

        Assert.Contains(descriptors, d => d.RequestType == typeof(AgentMarkerCommand));
    }

    [Fact]
    public void ScanTypes_MarkedDiscovery_ExcludesUntagged()
    {
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentUntaggedCommand)], new CqrsAgentOptions(), XmlDoc);

        Assert.DoesNotContain(descriptors, d => d.RequestType == typeof(AgentUntaggedCommand));
    }

    [Fact]
    public void ScanTypes_MarkedDiscovery_ExcludesExcludedAttribute()
    {
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentExcludedCommand)], new CqrsAgentOptions(), XmlDoc);

        Assert.DoesNotContain(descriptors, d => d.RequestType == typeof(AgentExcludedCommand));
    }

    [Fact]
    public void ScanTypes_AllValidRequests_IncludesUntagged()
    {
        var options = new CqrsAgentOptions { Discovery = ToolDiscovery.AllValidRequests };
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentUntaggedCommand)], options, XmlDoc);

        Assert.Contains(descriptors, d => d.RequestType == typeof(AgentUntaggedCommand));
    }

    [Fact]
    public void ScanTypes_AllValidRequests_ExcludesExcludedAttribute()
    {
        var options = new CqrsAgentOptions { Discovery = ToolDiscovery.AllValidRequests };
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentExcludedCommand)], options, XmlDoc);

        Assert.DoesNotContain(descriptors, d => d.RequestType == typeof(AgentExcludedCommand));
    }

    [Fact]
    public void ScanTypes_DuplicateNames_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CqrsToolScanner.ScanTypes(
                [typeof(CollisionSetA.CollideCommand), typeof(CollisionSetB.CollideCommand)],
                new CqrsAgentOptions(),
                XmlDoc));
    }

    [Fact]
    public void ScanTypes_NonRequestType_Skipped()
    {
        var descriptors = CqrsToolScanner.ScanTypes([typeof(string)], new CqrsAgentOptions(), XmlDoc);

        Assert.Empty(descriptors);
    }
}
