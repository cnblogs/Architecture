using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
/// Converter between long and string
/// </summary>
internal class LongToStringConverter : JsonConverter<long>
{
    /// <inheritdoc />
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return reader.GetInt64();
        }

        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException("string is empty");
        }

        var success = long.TryParse(raw, out var parsed);
        if (success == false)
        {
            throw new JsonException("string value can't be converted to long");
        }

        return parsed;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
