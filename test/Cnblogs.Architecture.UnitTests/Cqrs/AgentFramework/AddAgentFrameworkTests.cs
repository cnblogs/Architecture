using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class AddAgentFrameworkTests
{
    [Fact]
    public void AddAgentFramework_RegistersToolSetOptionsRegistryAndFactory()
    {
        var services = new ServiceCollection();
        services.AddCqrs(typeof(AddAgentFrameworkTests).Assembly).AddAgentFramework();

        Assert.Contains(services, d => d.ServiceType == typeof(CqrsToolSet));
        Assert.Contains(services, d => d.ServiceType == typeof(CqrsAgentOptions));
        Assert.Contains(services, d => d.ServiceType == typeof(CqrsAgentRegistry));
        Assert.Contains(services, d => d.ServiceType == typeof(CqrsAgentFactory));
    }

    [Fact]
    public void AddAgentFramework_DiscoversAndRegistersTypedAgents()
    {
        var services = new ServiceCollection();
        services.AddCqrs(typeof(AddAgentFrameworkTests).Assembly).AddAgentFramework();
        var serviceProvider = services.BuildServiceProvider();

        var registry = serviceProvider.GetRequiredService<CqrsAgentRegistry>();

        Assert.Contains(typeof(AgentTestAgent), registry.AgentTypes);
        Assert.Contains(typeof(AgentOpenAgent), registry.AgentTypes);
        Assert.IsType<AgentTestAgent>(serviceProvider.GetRequiredService<AgentTestAgent>());
    }
}
