using Cnblogs.Architecture.Ddd.Cqrs.Mcp;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Mcp;

public class AddMcpTests
{
    [Fact]
    public void AddMcp_RegistersToolCollectionOptionsSetupAndHttpContextAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(typeof(AddMcpTests).Assembly).AddMcp();

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(CqrsMcpToolCollection));
        Assert.Contains(services, d => d.ServiceType == typeof(IHttpContextAccessor));
        Assert.Contains(
            services,
            d => d is { ServiceType: var serviceType, ImplementationType: { } implementationType }
                && serviceType == typeof(IConfigureOptions<McpServerOptions>)
                && implementationType == typeof(CqrsMcpOptionsSetup));
    }

    [Fact]
    public void AddMcp_CanBeCalledTwice()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(typeof(AddMcpTests).Assembly).AddMcp().AddMcp();

        // Assert
        Assert.Equal(1, services.Count(d => d.ServiceType == typeof(CqrsMcpToolCollection)));
    }
}
