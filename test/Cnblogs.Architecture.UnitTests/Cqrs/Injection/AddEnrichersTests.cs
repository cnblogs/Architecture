using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Injection;

public class AddEnrichersTests
{
    public interface IContainUserInfo;

    public record UserDto : IContainUserInfo;
    public record AdminDto : IContainUserInfo;

    // ReSharper disable once UnusedType.Global
    public class UserInfoEnricher : IEnricher<IContainUserInfo>
    {
        public bool AllowParallel => false;

        public Task EnrichAsync(IContainUserInfo? model, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task BulkEnrichAsync(IEnumerable<IContainUserInfo?> models, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void InterfaceEnricher_InterfaceBasedEnricher_RegisteredForAllImplementations()
    {
        // Arrange
        var services = new ServiceCollection();
        var injector = services.AddCqrs(typeof(AddEnrichersTests).Assembly);
        injector.AddEnrichers([typeof(AddEnrichersTests).Assembly]);

        // Act
        var sp = services.BuildServiceProvider();
        var userEnrichers = sp.GetServices<IEnricher<UserDto>>().ToList();
        var adminEnrichers = sp.GetServices<IEnricher<AdminDto>>().ToList();

        // Assert
        Assert.Single(userEnrichers);
        Assert.Single(adminEnrichers);
    }

    [Fact]
    public void ConcreteEnricher_ConcreteTypeEnricher_RegisteredDirectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var injector = services.AddCqrs(typeof(AddEnrichersTests).Assembly);
        injector.AddEnrichers([typeof(AddEnrichersTests).Assembly]);

        // Act
        var sp = services.BuildServiceProvider();
        var enrichers = sp.GetServices<IEnricher<FakePostDto>>().ToList();

        // Assert
        Assert.Contains(enrichers, e => e is TrackingEnricher);
    }

    [Fact]
    public void InterfaceEnricher_ExceedsLimit_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var injector = services.AddCqrs(typeof(AddEnrichersTests).Assembly);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            injector.AddEnrichers([typeof(AddEnrichersTests).Assembly], maxInterfaceImplementations: 1));
    }
}
