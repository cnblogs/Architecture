using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using MediatR;
using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Holds the per-request-type metadata, JSON schema, argument-binding and result-marshalling logic for one agent tool.
///     The schema is derived from the request record's constructor parameters via <see cref="AIJsonUtilities" />; CQRS
///     infrastructure parameters are hidden and paging is flattened.
/// </summary>
internal sealed class CqrsToolDescriptor
{
    /// <summary>
    ///     Creates a descriptor for <paramref name="requestType" />.
    /// </summary>
    public CqrsToolDescriptor(Type requestType, RequestKind kind, CqrsAgentOptions options, XmlDocumentationProvider xmlDoc)
    {
        RequestType = requestType;
        Kind = kind;
        var attribute = requestType.GetCustomAttribute<AgentToolAttribute>(inherit: true);
        AllowValidateOnly = attribute?.AllowValidateOnly ?? false;
        IsPageable = CqrsRequestInspector.IsPageable(requestType);
        HideInfrastructure = options.HideCqrsInfrastructureParameters;
        MaxPageSize = options.MaxPageSize;
        SerializerOptions = options.SerializerOptions ?? AIJsonUtilities.DefaultOptions;
        Name = attribute?.Name ?? requestType.Name;
        Description = attribute?.Description ?? xmlDoc.GetTypeSummary(requestType) ?? string.Empty;
        JsonSchema = BuildSchema(requestType, xmlDoc);
    }

    /// <summary>The CQRS request type.</summary>
    public Type RequestType { get; }

    /// <summary>Whether the request is a Command or a Query.</summary>
    public RequestKind Kind { get; }

    /// <summary>The tool name (record type name unless overridden).</summary>
    public string Name { get; }

    /// <summary>The tool description (XML doc summary unless overridden).</summary>
    public string Description { get; }

    /// <summary>The JSON schema for the tool's arguments.</summary>
    public JsonElement JsonSchema { get; }

    /// <summary>Whether the request is pageable.</summary>
    public bool IsPageable { get; }

    /// <summary>Whether <c>ValidateOnly</c> may be set by the model.</summary>
    public bool AllowValidateOnly { get; }

    /// <summary>Whether CQRS infrastructure parameters are hidden from the schema.</summary>
    public bool HideInfrastructure { get; }

    /// <summary>The maximum page size the model may request.</summary>
    public int MaxPageSize { get; }

    /// <summary>The serializer options used for binding and marshalling.</summary>
    public JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    ///     Marshals the <see cref="IMediator" /> response into an LLM-consumable result, mirroring the branching in
    ///     <c>CommandEndpointHandler</c>/<c>QueryEndpointHandler</c>.
    /// </summary>
    public static object MarshalResult(object? response)
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

    /// <summary>
    ///     Rebuilds the request record from the model-supplied arguments, hiding infrastructure parameters and reconstructing
    ///     <c>PagingParams</c> for pageable queries.
    /// </summary>
    public object BindRequest(AIFunctionArguments arguments)
    {
        var obj = new JsonObject();
        foreach (var (key, value) in arguments)
        {
            if (HideInfrastructure && IsHiddenInfrastructure(key, AllowValidateOnly))
            {
                continue;
            }

            obj[key] = ToJsonNode(value);
        }

        if (IsPageable && HideInfrastructure)
        {
            var pageIndex = Math.Max(1, GetInt(arguments, "pageIndex", 1));
            var pageSize = Math.Clamp(GetInt(arguments, "pageSize", MaxPageSize), 1, MaxPageSize);
            obj["pagingParams"] = new JsonObject { ["pageIndex"] = pageIndex, ["pageSize"] = pageSize };
        }

        return JsonSerializer.Deserialize(obj.ToJsonString(), RequestType, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to bind agent tool arguments to {RequestType.FullName}.");
    }

    private JsonElement BuildSchema(Type requestType, XmlDocumentationProvider xmlDoc)
    {
        var schema = AIJsonUtilities.CreateJsonSchema(requestType, serializerOptions: SerializerOptions);
        var node = JsonNode.Parse(schema.GetRawText())?.AsObject() ?? new JsonObject();
        var properties = node["properties"]?.AsObject();
        var required = node["required"]?.AsArray();

        var constructor = requestType.GetConstructors().MaxBy(c => c.GetParameters().Length);
        var parameterDocs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (constructor is not null)
        {
            foreach (var parameter in constructor.GetParameters())
            {
                var description = xmlDoc.GetParameterDescription(parameter);
                if (string.IsNullOrWhiteSpace(description) == false)
                {
                    parameterDocs[parameter.Name!] = description;
                }
            }
        }

        if (properties is not null)
        {
            foreach (var entry in properties.ToArray())
            {
                var key = entry.Key;
                if (HideInfrastructure && IsHiddenInfrastructure(key, AllowValidateOnly))
                {
                    properties.Remove(key);
                    RemoveRequired(required, key);
                    continue;
                }

                if (parameterDocs.TryGetValue(key, out var description) && properties[key] is JsonObject propertySchema)
                {
                    propertySchema["description"] = description;
                }
            }
        }

        if (IsPageable && HideInfrastructure && properties is not null)
        {
            properties["pageIndex"] = new JsonObject { ["type"] = "integer", ["description"] = "1-based page index.", ["default"] = 1 };
            properties["pageSize"] = new JsonObject { ["type"] = "integer", ["description"] = $"Page size (capped at {MaxPageSize}).", ["default"] = MaxPageSize };
        }

        return ToJsonElement(node);
    }

    private static bool IsHiddenInfrastructure(string propertyName, bool allowValidateOnly)
    {
        if (string.Equals(propertyName, "validateOnly", StringComparison.OrdinalIgnoreCase))
        {
            return allowValidateOnly == false;
        }

        return string.Equals(propertyName, "orderByString", StringComparison.OrdinalIgnoreCase)
               || string.Equals(propertyName, "pagingParams", StringComparison.OrdinalIgnoreCase);
    }

    private static void RemoveRequired(JsonArray? required, string key)
    {
        if (required is null)
        {
            return;
        }

        for (var i = required.Count - 1; i >= 0; i--)
        {
            if (required[i]?.GetValue<string>() == key)
            {
                required.RemoveAt(i);
            }
        }
    }

    private static int GetInt(AIFunctionArguments arguments, string key, int defaultValue)
    {
        if (arguments.TryGetValue(key, out var value) is false || value is null)
        {
            return defaultValue;
        }

        return value switch
        {
            JsonElement e => e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var i) ? i : defaultValue,
            int i => i,
            _ => int.TryParse(value.ToString(), out var parsed) ? parsed : defaultValue,
        };
    }

    private static JsonNode? ToJsonNode(object? value)
    {
        return value switch
        {
            null => null,
            JsonElement element => JsonNode.Parse(element.GetRawText()),
            JsonNode node => node.DeepClone(),
            _ => JsonSerializer.SerializeToNode(value),
        };
    }

    private static JsonElement ToJsonElement(JsonNode node)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
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
