using Cnblogs.Architecture.TestIntegrationEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.EventBus;

public class AssemblyAttributeTests
{
    [Fact]
    public void SubscribeByAssemblyMeta_Success()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDaprEventBus(nameof(AssemblyAttributeTests));
        var app = builder.Build();

        // Act
        var act = () => app.Subscribe<TestIntegrationEvent>();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SubscribeByAssemblyMeta_Throw()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var act = () => app.Subscribe<TestIntegrationEvent>();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}