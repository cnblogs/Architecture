using System.Text.Json;
using System.Text.Json.Serialization;
using Cnblogs.Architecture.Tool.Manifest;

namespace Cnblogs.Architecture.Tool;

internal static class ManifestReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Deserialize the manifest at <paramref name="path" />.</summary>
    public static EndpointManifest Read(string path)
    {
        var json = File.ReadAllText(path);
        var manifest = JsonSerializer.Deserialize<EndpointManifest>(json, Options);
        if (manifest is null)
        {
            throw new InvalidDataException($"The manifest at '{path}' is empty or could not be parsed.");
        }

        return manifest;
    }
}
