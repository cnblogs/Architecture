using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>Serializes an <see cref="EndpointManifest" /> to a JSON file.</summary>
public static class EndpointManifestWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Write <paramref name="manifest" /> to <paramref name="path" />, creating parent directories as needed.</summary>
    /// <param name="path">The output file path.</param>
    /// <param name="manifest">The manifest to serialize.</param>
    public static void Write(string path, EndpointManifest manifest)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(manifest, Options));
    }
}
