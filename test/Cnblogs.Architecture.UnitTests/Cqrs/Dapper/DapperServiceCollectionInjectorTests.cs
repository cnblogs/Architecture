using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class DapperServiceCollectionInjectorTests
{
    [Fact]
    public void AddDapperContext_AddContext_Success()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDapperContext<TestDapperContext>();

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(TestDapperContext));
    }

    [Fact]
    public void AddDapperContext_AddSameContextMultipleTimes_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDapperContext<TestDapperContext>();

        // Assert
        Assert.Throws<InvalidOperationException>(() => services.AddDapperContext<TestDapperContext>());
    }
}
