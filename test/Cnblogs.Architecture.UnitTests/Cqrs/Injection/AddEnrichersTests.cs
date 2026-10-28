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

        public Task BulkEnrichAsync(IEnumerable<IContainUserInfo> models, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    // ReSharper disable once UnusedType.Global
    public class MultiTargetEnricher : IEnricher<UserDto>, IEnricher<AdminDto>
    {
        public bool AllowParallel => false;

        public Task EnrichAsync(UserDto model, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnrichAsync(AdminDto model, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void InterfaceEnricher_InterfaceBasedEnricher_PlanBuiltForConcreteTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var injector = services.AddCqrs(typeof(AddEnrichersTests).Assembly);
        injector.AddEnrichers(new List<Type> { typeof(UserInfoEnricher) });

        // Act
        var sp = services.BuildServiceProvider();
        var enricher = sp.GetService<UserInfoEnricher>();

        // Assert — enricher is registered and can be resolved by impl type
        Assert.NotNull(enricher);
    }

    [Fact]
    public void ConcreteEnricher_ConcreteTypeEnricher_RegisteredDirectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var injector = services.AddCqrs(typeof(AddEnrichersTests).Assembly);
        injector.AddEnrichers(new List<Type> { typeof(TrackingEnricher) });

        // Act
        var sp = services.BuildServiceProvider();
        var enricher = sp.GetService<TrackingEnricher>();

        // Assert
        Assert.NotNull(enricher);
    }

    [Fact]
    public void MultiInterfaceEnricher_ImplementsMultipleClosures_PlansBuiltForAllTargets()
    {
        // Arrange
        var services = new ServiceCollection();
        var injector = services.AddCqrs(typeof(AddEnrichersTests).Assembly);

        // Act — plan pre-warm resolves EnrichAsync/BulkEnrichAsync per target without error
        injector.AddEnrichers(new List<Type> { typeof(MultiTargetEnricher) });
        var sp = services.BuildServiceProvider();
        var enricher = sp.GetService<MultiTargetEnricher>();

        // Assert
        Assert.NotNull(enricher);
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