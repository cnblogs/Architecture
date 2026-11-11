using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.Ddd.Cqrs.Mcp;
using Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Mcp;

public class AddMcpTests
{
    private static readonly XmlDocumentationProvider XmlDoc = new();

    [Fact]
    public void AddMcp_RegistersAgentFrameworkAndMcpServicesAutomatically()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(typeof(AddMcpTests).Assembly).AddMcp(o => o.AnnotateQueriesAsReadOnly = false);

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(CqrsToolSet));
        Assert.Contains(services, d => d.ServiceType == typeof(CqrsAgentOptions));
        Assert.Contains(services, d => d.ServiceType == typeof(IConfigureOptions<CqrsMcpOptions>));
        Assert.Contains(
            services,
            d => d is { ServiceType: var serviceType, ImplementationType: { } implementationType }
                && serviceType == typeof(IConfigureOptions<McpServerOptions>)
                && implementationType == typeof(CqrsMcpOptionsSetup));
    }

    [Fact]
    public void AddMcp_DoesNotDuplicateAgentFrameworkRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(typeof(AddMcpTests).Assembly)
            .AddAgentFramework()
            .AddMcp();

        // Assert
        Assert.Equal(1, services.Count(d => d.ServiceType == typeof(CqrsAgentOptions)));
    }

    [Fact]
    public void WithCqrsTools_ExposesMarkedCommandsAndQueriesAsTools()
    {
        // Arrange
        using var serviceProvider = BuildProvider();

        // Act
        var tools = serviceProvider.GetRequiredService<IOptions<McpServerOptions>>().Value.ToolCollection!;

        // Assert
        Assert.NotNull(tools);
        Assert.Contains(tools, t => t.ProtocolTool.Name == nameof(AgentCreateCommand));
        Assert.Contains(tools, t => t.ProtocolTool.Name == nameof(AgentItemQuery));
        Assert.DoesNotContain(tools, t => t.ProtocolTool.Name == nameof(AgentUntaggedCommand));
    }

    [Fact]
    public void WithCqrsTools_AnnotatesQueriesAsReadOnlyAndLeavesCommandsUnannotated()
    {
        // Arrange
        using var serviceProvider = BuildProvider();
        var tools = serviceProvider.GetRequiredService<IOptions<McpServerOptions>>().Value.ToolCollection!;

        // Act
        var query = tools[nameof(AgentItemQuery)];
        var command = tools[nameof(AgentCreateCommand)];

        // Assert
        Assert.True(query.ProtocolTool.Annotations?.ReadOnlyHint);
        Assert.False(command.ProtocolTool.Annotations?.ReadOnlyHint ?? false);
    }

    [Fact]
    public void WithCqrsTools_RespectsRequestFilter()
    {
        // Arrange
        using var serviceProvider = BuildProvider(o => o.RequestFilter = t => t == typeof(AgentItemQuery));

        // Act
        var tools = serviceProvider.GetRequiredService<IOptions<McpServerOptions>>().Value.ToolCollection!;

        // Assert
        var tool = Assert.Single(tools);
        Assert.Equal(nameof(AgentItemQuery), tool.ProtocolTool.Name);
    }

    [Fact]
    public void WithCqrsTools_Throws_WhenAgentFrameworkIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddMcpServer().WithCqrsTools();

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    private static ServiceProvider BuildProvider(Action<CqrsMcpOptions>? configure = null)
    {
        var services = new ServiceCollection();

        // Build the tool set from an explicit type list (instead of AddCqrs(assembly), which would hit the
        // intentionally-colliding CollideCommand fixtures), the same way CqrsAgentFactoryTests does.
        var descriptors = CqrsToolScanner.ScanTypes(
            [typeof(AgentCreateCommand), typeof(AgentItemQuery), typeof(AgentUntaggedCommand)],
            new CqrsAgentOptions(),
            XmlDoc);
        var byRequestType = descriptors.ToDictionary(d => d.RequestType, d => (AIFunction)new CqrsToolFunction(d));
        services.AddSingleton(new CqrsToolSet([.. byRequestType.Values], byRequestType));

        services.AddMcpServer()
            .WithHttpTransport()
            .WithCqrsTools(configure);
        return services.BuildServiceProvider();
    }
}
