using System.Runtime.CompilerServices;

using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

/// <summary>
///     A scriptable <see cref="IChatClient" /> for agent tests. Returns queued <see cref="ChatResponse" />s in call order
///     (repeating the last response once the script is exhausted) and records every call's messages and options. Use
/// <see cref="EnqueueText" /> for plain replies, or <see cref="Enqueue" /> for a full response (e.g. carrying tool calls)
/// to drive multi-turn tool loops.
/// </summary>
public sealed class StubChatClient : IChatClient
{
    private static readonly ChatResponse Empty = new(new ChatMessage(ChatRole.Assistant, string.Empty));

    private readonly List<ChatResponse> _script = [];
    private readonly List<Call> _calls = [];
    private ChatResponse? _last;

    /// <summary>The calls received, in order (each call's messages and options).</summary>
    public IReadOnlyList<Call> Calls => _calls;

    /// <summary>The number of times the client was invoked.</summary>
    public int CallCount => _calls.Count;

    /// <summary>Enqueues a canned full response; the agent receives responses in enqueue order.</summary>
    /// <param name="response">The response to return for the next call that reaches this position.</param>
    /// <returns>This instance, for chaining.</returns>
    public StubChatClient Enqueue(ChatResponse response)
    {
        _script.Add(response);
        _last = response;
        return this;
    }

    /// <summary>Enqueues a plain assistant text reply.</summary>
    /// <param name="text">The reply text.</param>
    /// <returns>This instance, for chaining.</returns>
    public StubChatClient EnqueueText(string text)
        => Enqueue(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));

    /// <summary>
    ///     Enqueues an assistant response that requests a tool call, so a wrapping <c>FunctionInvokingChatClient</c> invokes the matching
    ///     tool (dispatching it in-process through <c>IMediator</c>) before the next scripted response is returned.
    /// </summary>
    /// <param name="callId">A unique id for this tool call.</param>
    /// <param name="toolName">The tool name to invoke; must match a registered tool (the CQRS request type name).</param>
    /// <param name="arguments">The arguments to bind to the request record.</param>
    /// <returns>This instance, for chaining.</returns>
    public StubChatClient EnqueueToolCall(string callId, string toolName, IDictionary<string, object?> arguments)
        => Enqueue(new ChatResponse(new ChatMessage(ChatRole.Assistant, [new FunctionCallContent(callId, toolName, arguments)])));

    /// <inheritdoc />
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Record(messages, options);
        return Task.FromResult(SelectResponse());
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Record(messages, options);
        foreach (var message in SelectResponse().Messages)
        {
            await Task.Yield();
            yield return new ChatResponseUpdate(message.Role, message.Text);
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // no-op
    }

    private void Record(IEnumerable<ChatMessage> messages, ChatOptions? options)
    {
        _calls.Add(new Call(messages.ToList(), options));
    }

    private ChatResponse SelectResponse()
    {
        var index = _calls.Count - 1;
        if (index < _script.Count)
        {
            return _script[index];
        }

        return _last ?? Empty;
    }

    /// <summary>A recorded invocation of the chat client.</summary>
    /// <param name="Messages">The messages sent in this call.</param>
    /// <param name="Options">The chat options sent in this call, if any.</param>
    public sealed record Call(IReadOnlyList<ChatMessage> Messages, ChatOptions? Options);
}
