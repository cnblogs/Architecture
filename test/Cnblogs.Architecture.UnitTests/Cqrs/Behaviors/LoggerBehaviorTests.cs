using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

using Microsoft.Extensions.Logging;

using Moq;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class LoggerBehaviorTests
{
    [Fact]
    public async Task LoggerBehavior_ShouldLogDebugAsync()
    {
        // Arrange
        var logger = new Mock<ILogger<LoggingBehavior<FakeQuery<string>, string>>>();
        var behavior = new LoggingBehavior<FakeQuery<string>, string>(logger.Object);
        var request = new FakeQuery<string>(null, "test");

        // Act
        await behavior.Handle(request, () => Task.FromResult("done"), default);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }
}