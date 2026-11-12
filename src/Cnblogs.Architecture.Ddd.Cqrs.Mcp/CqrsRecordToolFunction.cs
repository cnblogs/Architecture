using System.Text.Json;

using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     An <see cref="AIFunction" /> that binds tool arguments to a CQRS request record via
///     <see cref="CqrsToolRequestBinder{TRequest}" /> and dispatches it in-process via <c>IMediator</c> using the MCP
///     request's service provider.
/// </summary>
internal sealed class CqrsRecordToolFunction<TRequest>(string name, string description) : AIFunction
{
    /// <inheritdoc />
    public override string Name { get; } = name;

    /// <inheritdoc />
    public override string Description { get; } = description;

    /// <inheritdoc />
    public override JsonElement JsonSchema => CqrsToolRequestBinder<TRequest>.Schema;

    /// <inheritdoc />
    public override JsonSerializerOptions JsonSerializerOptions => CqrsToolRequestBinder<TRequest>.SerializerOptions;

    /// <inheritdoc />
    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var services = arguments.Services
            ?? throw new InvalidOperationException(
                $"{nameof(CqrsRecordToolFunction<>)} requires {nameof(AIFunctionArguments.Services)} to be set, "
                + "which the MCP server does automatically for each tool call.");

        var request = CqrsToolRequestBinder<TRequest>.Bind(arguments);
        return await CqrsToolDispatcher.DispatchAsync(request, services, cancellationToken);
    }
}
