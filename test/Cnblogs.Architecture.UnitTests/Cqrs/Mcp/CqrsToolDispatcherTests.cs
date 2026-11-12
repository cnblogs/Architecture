using System.Text.Json;

using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.Mcp;
using Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Mcp;

public class CqrsToolDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_ReturnsQueryResultAsIsAsync()
    {
        // Arrange
        object? sent = null;
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                sent = callInfo.Args()[0];
                return Task.FromResult<object?>("the view");
            });
        var provider = BuildProvider(mediator);

        // Act
        var result = await CqrsToolDispatcher.DispatchAsync(new McpGetThingQuery(1), provider, CancellationToken.None);

        // Assert
        Assert.Equal("the view", result);
        var query = Assert.IsType<McpGetThingQuery>(sent);
        Assert.Equal(1, query.Id);
    }

    [Fact]
    public async Task DispatchAsync_ReturnsOkForSuccessfulCommandAsync()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(CommandResponse<AgentTestError>.Success()));
        var provider = BuildProvider(mediator);

        // Act
        var result = await CqrsToolDispatcher.DispatchAsync(new McpCreateThingCommand("t"), provider, CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task DispatchAsync_ReturnsViewForSuccessfulCommandWithResultAsync()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(CommandResponse<string, AgentTestError>.Success("created")));
        var provider = BuildProvider(mediator);

        // Act
        var result = await CqrsToolDispatcher.DispatchAsync(new McpCreateThingCommand("t"), provider, CancellationToken.None);

        // Assert
        Assert.Equal("created", result);
    }

    [Fact]
    public async Task DispatchAsync_ReturnsStructuredValidationErrorsAsync()
    {
        // Arrange
        var validationErrors = new ValidationErrors { new("Title is required", nameof(McpCreateThingCommand.Title)) };
        var response = new CommandResponse<AgentTestError>
        {
            IsValidationError = true,
            ErrorMessage = "Validation failed",
            ValidationErrors = validationErrors,
        };
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(response));
        var provider = BuildProvider(mediator);

        // Act
        var result = await CqrsToolDispatcher.DispatchAsync(new McpCreateThingCommand(string.Empty), provider, CancellationToken.None);

        // Assert
        var json = JsonSerializer.SerializeToNode(result)!.AsObject();
        Assert.Equal("validation", json["error"]!.GetValue<string>());
        Assert.Equal("Validation failed", json["message"]!.GetValue<string>());
        Assert.Equal("Title is required", json["fields"]!["Title"]![0]!.GetValue<string>());
    }

    [Fact]
    public async Task DispatchAsync_ReturnsNotFoundForNullResponseAsync()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(null));
        var provider = BuildProvider(mediator);

        // Act
        var result = await CqrsToolDispatcher.DispatchAsync(new McpGetThingQuery(1), provider, CancellationToken.None);

        // Assert
        Assert.Equal("not found", result);
    }

    private static IServiceProvider BuildProvider(IMediator mediator)
        => new ServiceCollection().AddScoped(_ => mediator).BuildServiceProvider();
}
