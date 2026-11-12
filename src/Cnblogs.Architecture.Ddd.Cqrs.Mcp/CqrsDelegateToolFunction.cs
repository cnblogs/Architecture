using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     An <see cref="AIFunction" /> that wraps the user's request factory delegate (argument binding and schema
///     generation are delegated to <see cref="AIFunctionFactory" />, preserving the factory's parameter names) and
///     dispatches the built request in-process via <c>IMediator</c> using the MCP request's service provider.
///     <see cref="ClaimsPrincipal" />, MCP request context types, DI-registered services and <c>CancellationToken</c>
///     parameters are injected and hidden from the schema.
/// </summary>
internal sealed class CqrsDelegateToolFunction<TRequest>(
    Delegate factory,
    string name,
    string description,
    IServiceProvider? applicationServices) : AIFunction
{
    private readonly AIFunction _inner = AIFunctionFactory.Create(
        factory,
        new AIFunctionFactoryOptions
        {
            Name = name,
            Description = description,
            // Return the request object as-is instead of the default JSON-serialized JsonElement,
            // so it can be cast to TRequest and dispatched below.
            MarshalResult = static (result, _, _) => new ValueTask<object?>(result),
            ConfigureParameterBinding = parameter => ConfigureParameterBinding(parameter, applicationServices),
        });

    /// <inheritdoc />
    public override string Name => _inner.Name;

    /// <inheritdoc />
    public override string Description => _inner.Description;

    /// <inheritdoc />
    public override JsonElement JsonSchema => _inner.JsonSchema;

    /// <inheritdoc />
    public override JsonSerializerOptions JsonSerializerOptions => _inner.JsonSerializerOptions;

    /// <inheritdoc />
    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var services = arguments.Services
            ?? throw new InvalidOperationException(
                $"{nameof(CqrsDelegateToolFunction<TRequest>)} requires {nameof(AIFunctionArguments.Services)} to be set, "
                + "which the MCP server does automatically for each tool call.");

        var request = (TRequest)(await _inner.InvokeAsync(arguments, cancellationToken).ConfigureAwait(false))!;
        return await CqrsToolDispatcher.DispatchAsync(request, services, cancellationToken).ConfigureAwait(false);
    }

    private static AIFunctionFactoryOptions.ParameterBindingOptions ConfigureParameterBinding(
        ParameterInfo parameter,
        IServiceProvider? applicationServices)
    {
        var parameterType = parameter.ParameterType;
        var isInjected = parameterType == typeof(ClaimsPrincipal)
                         || parameterType == typeof(RequestContext<CallToolRequestParams>)
                         || parameterType == typeof(McpServer)
                         || parameterType == typeof(IProgress<ProgressNotificationValue>)
                         || (applicationServices as IServiceProviderIsService)?.IsService(parameterType) is true;
        return isInjected
            ? new AIFunctionFactoryOptions.ParameterBindingOptions
            {
                ExcludeFromSchema = true,
                BindParameter = (_, args) => args.Services?.GetService(parameterType),
            }
            : default;
    }
}
