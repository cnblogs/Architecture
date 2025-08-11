using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Cnblogs.Architecture.TestIntegrationEvents;
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
        builder.Services.AddCqrs().AddEventBus(o => o.UseDapr(nameof(AssemblyAttributeTests)));
        var app = builder.Build();

        // Act
        app.Subscribe<TestIntegrationEvent>();

        // Assert-Not throws
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
        Assert.Throws<InvalidOperationException>(act);
    }
}
