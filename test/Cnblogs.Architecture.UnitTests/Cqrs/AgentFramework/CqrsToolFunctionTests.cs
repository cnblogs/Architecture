using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

using MediatR;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsToolFunctionTests
{
    private static CqrsToolDescriptor NewDescriptor()
    {
        return new CqrsToolDescriptor(typeof(AgentCreateCommand), RequestKind.Command, new CqrsAgentOptions(), new XmlDocumentationProvider());
    }

    [Fact]
    public async Task InvokeAsync_ResolvesScopedMediator_AndDispatchesAsync()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(CommandResponse<AgentTestError>.Success());
        var services = new ServiceCollection();
        services.AddScoped(_ => mediator);
        var serviceProvider = services.BuildServiceProvider();

        var function = new CqrsToolFunction(NewDescriptor());
        var arguments = new AIFunctionArguments { Services = serviceProvider, ["title"] = "t", ["count"] = 3 };

        // Act
        var result = await function.InvokeAsync(arguments);

        // Assert
        Assert.Equal("ok", result);
        await mediator.Received().Send(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_NullServices_ThrowsInvalidOperationExceptionAsync()
    {
        var function = new CqrsToolFunction(NewDescriptor());
        var arguments = new AIFunctionArguments { ["title"] = "t", ["count"] = 3 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => function.InvokeAsync(arguments).AsTask());
    }
}
