using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Injection;

public class AddCqrsTests
{
    [Fact]
    public void AddCqrs_CacheNotConfigured_FrameworkCacheHandlerNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs();
        using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        var handler = sp.GetService<IRequestHandler<InvalidCacheRequest>>();

        // Assert
        Assert.Null(handler);
    }

    [Fact]
    public void AddHybridQueryCache_CacheConfigured_FrameworkCacheHandlersRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCqrs().AddHybridQueryCache();
        using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        using var scope = sp.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<InvalidCacheRequest>>();
        var groupHandler = scope.ServiceProvider.GetRequiredService<IRequestHandler<InvalidCacheGroupsRequest>>();

        // Assert
        Assert.IsType<InvalidCacheRequestHandler>(handler);
        Assert.IsType<InvalidCacheRequestHandler>(groupHandler);
    }

    [Fact]
    public void AddHybridQueryCache_UserScannedFrameworkAssembly_FrameworkCacheHandlerRegisteredOnce()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(typeof(InvalidCacheRequestHandler).Assembly).AddHybridQueryCache();
        var count = services.Count(x => x.ServiceType == typeof(IRequestHandler<InvalidCacheRequest>));

        // Assert
        Assert.Equal(1, count);
    }
}
