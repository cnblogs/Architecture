namespace Cnblogs.Architecture.Tool.Generation;

/// <summary>A source file produced by the emitter.</summary>
internal sealed class EmittedFile
{
    /// <summary>The file name (no directory) within the output project.</summary>
    public required string FileName { get; init; }

    /// <summary>The generated C# source.</summary>
    public required string Content { get; init; }

    /// <summary>Whether this is the DI-extensions file (registered under a fixed name and merged across groups).</summary>
    public bool IsExtensionsFile { get; init; }
}
