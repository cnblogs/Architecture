using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Builds the flattened JSON schema for a record-bound tool and binds model-supplied arguments back to the request
///     record. CQRS infrastructure parameters (<c>ValidateOnly</c>, <c>OrderByString</c>, <c>PagingParams</c>) are hidden from the
///     schema and bound server-side; paging is flattened to <c>pageIndex</c>/<c>pageSize</c> with the page size capped. Parameter
///     descriptions come from the request record's XML doc <c>&lt;param&gt;</c> comments. When you need any of those parameters
///     exposed, or context values (like a user id) injected, use the delegate overloads of <c>MapMcpTool</c> instead.
/// </summary>
internal static class CqrsToolRequestBinder<TRequest>
{
    private static readonly XmlDocumentationProvider XmlDocumentation = new();

    /// <summary>The maximum <c>pageSize</c> the model may request, to bound token cost.</summary>
    public const int MaxPageSize = 20;

    static CqrsToolRequestBinder()
    {
        SerializerOptions = AIJsonUtilities.DefaultOptions;
        IsPageable = CqrsRequestInspector.IsPageable(typeof(TRequest));
        Schema = BuildSchema();
    }

    private static bool IsPageable { get; }

    /// <summary>The JSON schema of the tool's arguments.</summary>
    public static JsonElement Schema { get; }

    /// <summary>The serializer options used for binding.</summary>
    public static JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    ///     The <c>&lt;summary&gt;</c> text of the request record, or <see langword="null" /> when unavailable. Used as the default
    ///     tool description when none is provided.
    /// </summary>
    public static string? TypeSummary { get; } = XmlDocumentation.GetTypeSummary(typeof(TRequest));

    /// <summary>
    ///     Rebuilds the request record from the model-supplied arguments, dropping infrastructure parameters and
    ///     reconstructing <c>PagingParams</c> for pageable queries.
    /// </summary>
    /// <param name="arguments">The model-supplied arguments.</param>
    /// <returns>The bound request.</returns>
    public static TRequest Bind(AIFunctionArguments arguments)
    {
        var obj = new JsonObject();
        foreach (var (key, value) in arguments)
        {
            if (IsHiddenInfrastructure(key))
            {
                continue;
            }

            obj[key] = ToJsonNode(value);
        }

        if (IsPageable)
        {
            var pageIndex = Math.Max(1, GetInt(arguments, "pageIndex", 1));
            var pageSize = Math.Clamp(GetInt(arguments, "pageSize", MaxPageSize), 1, MaxPageSize);
            obj["pagingParams"] = new JsonObject { ["pageIndex"] = pageIndex, ["pageSize"] = pageSize };
        }

        return JsonSerializer.Deserialize(obj.ToJsonString(), typeof(TRequest), SerializerOptions) is TRequest request
            ? request
            : throw new InvalidOperationException($"Failed to bind MCP tool arguments to {typeof(TRequest).FullName}.");
    }

    private static JsonElement BuildSchema()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(TRequest), serializerOptions: SerializerOptions);
        var node = JsonNode.Parse(schema.GetRawText())?.AsObject() ?? new JsonObject();
        var properties = node["properties"]?.AsObject();
        var required = node["required"]?.AsArray();

        var parameterDocs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var constructor = typeof(TRequest).GetConstructors().MaxBy(c => c.GetParameters().Length);
        if (constructor is not null)
        {
            foreach (var parameter in constructor.GetParameters())
            {
                var description = XmlDocumentation.GetParameterDescription(parameter);
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
                if (IsHiddenInfrastructure(entry.Key))
                {
                    properties.Remove(entry.Key);
                    RemoveRequired(required, entry.Key);
                    continue;
                }

                if (parameterDocs.TryGetValue(entry.Key, out var description) && properties[entry.Key] is JsonObject propertySchema)
                {
                    propertySchema["description"] = description;
                }
            }
        }

        if (IsPageable && properties is not null)
        {
            properties["pageIndex"] = new JsonObject { ["type"] = "integer", ["description"] = "1-based page index.", ["default"] = 1 };
            properties["pageSize"] = new JsonObject
            {
                ["type"] = "integer",
                ["description"] = $"Page size (capped at {MaxPageSize}).",
                ["default"] = MaxPageSize,
            };
        }

        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    private static bool IsHiddenInfrastructure(string propertyName)
    {
        return string.Equals(propertyName, "validateOnly", StringComparison.OrdinalIgnoreCase)
               || string.Equals(propertyName, "orderByString", StringComparison.OrdinalIgnoreCase)
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
}
