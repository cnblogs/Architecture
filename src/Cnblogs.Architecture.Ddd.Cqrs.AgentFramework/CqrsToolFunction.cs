using System.Text.Json;

using MediatR;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     An <see cref="AIFunction" /> that dispatches a CQRS Command or Query in-process via <c>IMediator</c>. Each call opens a
///     request scope from <see cref="AIFunctionArguments.Services" /> so the scoped MediatR pipeline (validation/logging/cache/lock/enricher)
///     and the handler run with the correct lifetimes.
/// </summary>
internal sealed class CqrsToolFunction : AIFunction
{
    private readonly CqrsToolDescriptor _descriptor;

    /// <summary>
    ///     Creates a function for the given descriptor.
    /// </summary>
    public CqrsToolFunction(CqrsToolDescriptor descriptor)
    {
        _descriptor = descriptor;
    }

    /// <inheritdoc />
    public override string Name => _descriptor.Name;

    /// <inheritdoc />
    public override string Description => _descriptor.Description;

    /// <inheritdoc />
    public override JsonElement JsonSchema => _descriptor.JsonSchema;

    /// <inheritdoc />
    public override JsonSerializerOptions JsonSerializerOptions => _descriptor.SerializerOptions;

    /// <inheritdoc />
    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var services = arguments.Services
            ?? throw new InvalidOperationException(
                $"{nameof(CqrsToolFunction)} requires {nameof(AIFunctionArguments.Services)} to be set. Construct the "
                + "FunctionInvokingChatClient with its functionInvocationServices argument (for example, pass HttpContext.RequestServices from the hosting endpoint).");

        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = _descriptor.BindRequest(arguments);
        var response = await mediator.Send(request, cancellationToken);
        return CqrsToolDescriptor.MarshalResult(response);
    }
}
