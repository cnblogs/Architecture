using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsRequestInspectorTests
{
    [Fact]
    public void GetKind_CommandInterface_ReturnsCommand()
    {
        Assert.Equal(RequestKind.Command, CqrsRequestInspector.GetKind(typeof(AgentCreateCommand)));
    }

    [Fact]
    public void GetKind_SingleItemQuery_ReturnsQuery()
    {
        Assert.Equal(RequestKind.Query, CqrsRequestInspector.GetKind(typeof(AgentItemQuery)));
    }

    [Fact]
    public void GetKind_PageableQuery_ReturnsQuery()
    {
        Assert.Equal(RequestKind.Query, CqrsRequestInspector.GetKind(typeof(AgentPagedQuery)));
    }

    [Fact]
    public void GetKind_NonRequest_ReturnsNone()
    {
        Assert.Equal(RequestKind.None, CqrsRequestInspector.GetKind(typeof(string)));
    }

    [Fact]
    public void IsPageable_PageableQuery_ReturnsTrue()
    {
        Assert.True(CqrsRequestInspector.IsPageable(typeof(AgentPagedQuery)));
    }

    [Fact]
    public void IsPageable_SingleItemQuery_ReturnsFalse()
    {
        Assert.False(CqrsRequestInspector.IsPageable(typeof(AgentItemQuery)));
    }
}
