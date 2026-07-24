using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

using MediatR;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsAgentFactoryTests
{
    private static readonly XmlDocumentationProvider XmlDoc = new();

    [Fact]
    public void GetCqrsAgent_RestrictedAgent_ReturnsChatClientAgent()
    {
        var serviceProvider = BuildProvider();

        var agent = serviceProvider.GetCqrsAgent<AgentTestAgent>();

        Assert.NotNull(agent);
        Assert.IsType<ChatClientAgent>(agent);
    }

    [Fact]
    public void GetCqrsAgent_OpenAgent_ReturnsChatClientAgent()
    {
        var serviceProvider = BuildProvider();

        var agent = serviceProvider.GetCqrsAgent<AgentOpenAgent>();

        Assert.NotNull(agent);
    }

    [Fact]
    public void GetCqrsAgent_CachesPerAgentType()
    {
        var serviceProvider = BuildProvider();

        var first = serviceProvider.GetCqrsAgent<AgentTestAgent>();
        var second = serviceProvider.GetCqrsAgent<AgentTestAgent>();

        Assert.Same(first, second);
    }

    [Fact]
    public async Task RunAsync_ReturnsStubbedReplyAsync()
    {
        var stub = new StubChatClient().EnqueueText("done");
        var serviceProvider = BuildProvider(stub);

        var agent = serviceProvider.GetCqrsAgent<AgentTestAgent>();
        var session = await agent.CreateSessionAsync(CancellationToken.None);
        var response = await agent.RunAsync("create an article", session, null, CancellationToken.None);

        Assert.Equal("done", response.Text);
        Assert.True(stub.CallCount > 0);
    }

    [Fact]
    public async Task RunAsync_DispatchesToolCallThroughMediatorAsync()
    {
        // First call: the "model" requests the AgentCreateCommand tool. FunctionInvokingChatClient then invokes it
        // in-process (mediator.Send) and appends the result. Second call: the "model" produces its final reply.
        var stub = new StubChatClient()
            .EnqueueToolCall("call_1", "AgentCreateCommand", new Dictionary<string, object?> { ["title"] = "t", ["count"] = 3 })
            .EnqueueText("created");
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(CommandResponse<AgentTestError>.Success());
        var serviceProvider = BuildProvider(stub, mediator);

        var agent = serviceProvider.GetCqrsAgent<AgentTestAgent>();
        var session = await agent.CreateSessionAsync(CancellationToken.None);
        var response = await agent.RunAsync("create an article titled t", session, null, CancellationToken.None);

        Assert.Equal("created", response.Text);
        await mediator.Received().Send(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Builds a minimal container with a curated (collision-free) tool set covering only <see cref="AgentCreateCommand" />,
    ///     so resolving the factory does not trip over the deliberate duplicate-name fixtures elsewhere in this assembly.
    /// </summary>
    /// <param name="chatClient">The <see cref="IChatClient" /> to register; defaults to a fresh <see cref="StubChatClient" />.</param>
    /// <param name="mediator">The <see cref="IMediator" /> to register; defaults to a fresh stub (never invoked unless a tool runs).</param>
    private static ServiceProvider BuildProvider(IChatClient? chatClient = null, IMediator? mediator = null)
    {
        var services = new ServiceCollection();
        var descriptors = CqrsToolScanner.ScanTypes([typeof(AgentCreateCommand)], new CqrsAgentOptions(), XmlDoc);
        var byRequestType = descriptors.ToDictionary(d => d.RequestType, d => (AIFunction)new CqrsToolFunction(d));
        services.AddSingleton(new CqrsToolSet([.. byRequestType.Values], byRequestType));
        services.AddSingleton(new CqrsAgentRegistry([typeof(AgentTestAgent), typeof(AgentOpenAgent)]));
        services.AddSingleton<AgentTestAgent>();
        services.AddSingleton<AgentOpenAgent>();
        services.AddSingleton<CqrsAgentFactory>();
        services.AddSingleton<IChatClient>(_ => chatClient ?? new StubChatClient());
        services.AddScoped(_ => mediator ?? Substitute.For<IMediator>());
        return services.BuildServiceProvider();
    }
}
