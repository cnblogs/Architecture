using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsAgentFactoryTests
{
    private static readonly XmlDocumentationProvider XmlDoc = new();

    [Fact]
    public void GetCqrsAgent_RestrictedAgent_ReturnsChatClientAgent()
    {
        var serviceProvider = BuildProvider();

        var agent = serviceProvider.GetCqrsAgent<AgentTestAgent>();

        Assert.NotNull(agent);
        Assert.IsType<ChatClientAgent>(agent);
    }

    [Fact]
    public void GetCqrsAgent_OpenAgent_ReturnsChatClientAgent()
    {
        var serviceProvider = BuildProvider();

        var agent = serviceProvider.GetCqrsAgent<AgentOpenAgent>();

        Assert.NotNull(agent);
    }

    [Fact]
    public void GetCqrsAgent_CachesPerAgentType()
    {
        var serviceProvider = BuildProvider();

        var first = serviceProvider.GetCqrsAgent<AgentTestAgent>();
        var second = serviceProvider.GetCqrsAgent<AgentTestAgent>();

        Assert.Same(first, second);
    }

    /// <summary>
    ///     Builds a minimal container with a curated (collision-free) tool set covering only <see cref="AgentCreateCommand" />,
    ///     so resolving the factory does not trip over the deliberate duplicate-name fixtures elsewhere in this assembly.
    /// </summary>
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentCreateCommand)], new CqrsAgentOptions(), XmlDoc);
        var byRequestType = descriptors.ToDictionary(d => d.RequestType, d => (AIFunction)new CqrsToolFunction(d));
        services.AddSingleton(new CqrsToolSet([.. byRequestType.Values], byRequestType));
        services.AddSingleton(new CqrsAgentRegistry([typeof(AgentTestAgent), typeof(AgentOpenAgent)]));
        services.AddSingleton<AgentTestAgent>();
        services.AddSingleton<AgentOpenAgent>();
        services.AddSingleton<CqrsAgentFactory>();
        services.AddSingleton<IChatClient>(_ => Substitute.For<IChatClient>());
        return services.BuildServiceProvider();
    }
}
