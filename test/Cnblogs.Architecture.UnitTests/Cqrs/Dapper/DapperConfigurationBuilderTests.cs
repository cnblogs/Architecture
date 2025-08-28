using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class DapperConfigurationBuilderTests
{
    [Fact]
    public void UseDbConnectionFactory_Instance_AddInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var factory = new TestDbConnectionFactory();

        // Act
        services.AddDapperContext<TestDapperContext>().UseDbConnectionFactory(factory);
        var serviceProvider = services.BuildServiceProvider();
        var fetchedFactory = serviceProvider.GetService<TestDbConnectionFactory>();
        var collection = serviceProvider.GetRequiredService<IOptions<DbConnectionFactoryCollection>>().Value;

        // Assert
        Assert.Equal(factory, fetchedFactory);
        Assert.Equal(typeof(TestDbConnectionFactory), collection.GetFactory(nameof(TestDapperContext)));
    }

    [Fact]
    public void UseDbConnectionFactory_Type_AddType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDapperContext<TestDapperContext>().UseDbConnectionFactory<TestDbConnectionFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var fetchedFactory = serviceProvider.GetService<TestDbConnectionFactory>();
        var collection = serviceProvider.GetRequiredService<IOptions<DbConnectionFactoryCollection>>().Value;

        // Assert
        Assert.NotNull(fetchedFactory);
        Assert.Equal(typeof(TestDbConnectionFactory), collection.GetFactory(nameof(TestDapperContext)));
    }

    [Fact]
    public void UseDbConnectionFactory_Func_AddFunc()
    {
        // Arrange
        var services = new ServiceCollection();
        var factory = new TestDbConnectionFactory();
        services.AddKeyedSingleton("test", factory);

        // Act
        services.AddDapperContext<TestDapperContext>()
            .UseDbConnectionFactory(sp => sp.GetRequiredKeyedService<TestDbConnectionFactory>("test"));
        var serviceProvider = services.BuildServiceProvider();
        var fetchedFactory = serviceProvider.GetRequiredService<TestDbConnectionFactory>();
        var collection = serviceProvider.GetRequiredService<IOptions<DbConnectionFactoryCollection>>().Value;

        // Assert
        Assert.Equal(factory, fetchedFactory);
        Assert.Equal(typeof(TestDbConnectionFactory), collection.GetFactory(nameof(TestDapperContext)));
    }
}
