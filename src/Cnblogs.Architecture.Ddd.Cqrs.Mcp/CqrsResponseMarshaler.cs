using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Marshals an <c>IMediator</c> response into an LLM-consumable result, mirroring the branching in
///     <c>CommandEndpointHandler</c>/<c>QueryEndpointHandler</c>: successful commands become <c>"ok"</c> (or their view),
///     missing results become <c>"not found"</c>, and errors become structured objects the model can act on.
/// </summary>
internal static class CqrsResponseMarshaler
{
    /// <summary>
    ///     Marshals the <c>IMediator</c> response for a tool call.
    /// </summary>
    /// <param name="response">The response returned by the handler.</param>
    /// <returns>The LLM-consumable result.</returns>
    public static object? Marshal(object? response)
    {
        if (response is null)
        {
            return "not found";
        }

        if (response is CommandResponse command)
        {
            if (command.IsSuccess())
            {
                return command is IObjectResponse objectResponse && objectResponse.GetResult() is { } result ? result : "ok";
            }

            return MarshalCommandError(command);
        }

        return response;
    }

    private static object MarshalCommandError(CommandResponse response)
    {
        if (response.IsValidationError)
        {
            var fields = response.ValidationErrors
                .GroupBy(e => e.ParameterName ?? "command")
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new { error = "validation", message = response.ErrorMessage, fields };
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return new { error = "concurrent", message = "Unable to acquire the lock within the time limit; please retry." };
        }

        return new { error = response.GetErrorMessage(), message = response.ErrorMessage };
    }
}
