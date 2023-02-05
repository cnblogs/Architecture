using Cnblogs.Architecture.TestIntegrationEvents;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Cnblogs.Architecture.UnitTests.EventBus;

public class AssemblyAttributeTests
{
    [Fact]
    public void SubscribeByAssemblyMeta_Success()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var act = () => app.Subscribe<TestIntegrationEvent>();

        // Assert
        act.Should().NotThrow();
    }
}