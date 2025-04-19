using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class LoggerBehaviorTests
{
    [Fact]
    public async Task LoggerBehavior_ShouldLogDebugAsync()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<FakeQuery<string>, string>>>();
        var behavior =
            new LoggingBehavior<FakeQuery<string>, string>(
                new TestLogger<LoggingBehavior<FakeQuery<string>, string>>(logger));
        var request = new FakeQuery<string>(null, "test");

        // Act
        await behavior.Handle(request, _ => Task.FromResult("done"), CancellationToken.None);

        // Assert
        logger.Received(2).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private class TestLogger<T> : ILogger<T>
    {
        private readonly ILogger<T> _logger;

        // ReSharper disable once ContextualLoggerProblem
        public TestLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        public virtual void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _logger.Log<object>(logLevel, eventId, state!, exception, (_, _) => string.Empty);
        }
    }
}
