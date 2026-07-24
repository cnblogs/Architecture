using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsAgentRegistryTests
{
    private static readonly XmlDocumentationProvider XmlDoc = new();

    private static IReadOnlyDictionary<Type, AIFunction> BuildByRequestType(params Type[] types)
    {
        var dict = new Dictionary<Type, AIFunction>();
        foreach (var type in types)
        {
            var descriptor = new CqrsToolDescriptor(type, CqrsRequestInspector.GetKind(type), new CqrsAgentOptions(), XmlDoc);
            dict[type] = new CqrsToolFunction(descriptor);
        }

        return dict;
    }

    [Fact]
    public void FilterTools_NullAllowed_ReturnsAll()
    {
        var byRequestType = BuildByRequestType(typeof(AgentCreateCommand), typeof(AgentItemQuery));
        var all = byRequestType.Values.ToList();

        var result = CqrsAgentRegistry.FilterTools(byRequestType, all, null, "a");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterTools_EmptyAllowed_ReturnsAll()
    {
        var byRequestType = BuildByRequestType(typeof(AgentCreateCommand), typeof(AgentItemQuery));
        var all = byRequestType.Values.ToList();

        var result = CqrsAgentRegistry.FilterTools(byRequestType, all, [], "a");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterTools_Subset_ReturnsOnlyListed()
    {
        var byRequestType = BuildByRequestType(typeof(AgentCreateCommand), typeof(AgentItemQuery));
        var all = byRequestType.Values.ToList();

        var result = CqrsAgentRegistry.FilterTools(byRequestType, all, [typeof(AgentCreateCommand)], "a");

        var single = Assert.Single(result);
        Assert.Equal(nameof(AgentCreateCommand), single.Name);
    }

    [Fact]
    public void FilterTools_UnknownType_ThrowsInvalidOperationException()
    {
        var byRequestType = BuildByRequestType(typeof(AgentCreateCommand));
        var all = byRequestType.Values.ToList();

        // AgentUntaggedCommand is a real command but not exposed as a tool.
        Assert.Throws<InvalidOperationException>(() =>
            CqrsAgentRegistry.FilterTools(byRequestType, all, [typeof(AgentUntaggedCommand)], "test-agent"));
    }
}
