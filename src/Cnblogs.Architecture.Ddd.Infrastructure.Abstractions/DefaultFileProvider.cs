using Stream = System.IO.Stream;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     Use default file provider.
/// </summary>
public class DefaultFileProvider : IFileProvider
{
    /// <inheritdoc />
    public Task<Stream> GetFileStreamAsync(string filename)
    {
        return Task.FromResult<Stream>(File.OpenRead(filename));
    }

    /// <inheritdoc />
    public async Task<byte[]> GetFileBytesAsync(string filename)
    {
        var file = await File.ReadAllBytesAsync(filename);
        return file;
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(string filename, Stream filestream)
    {
        var file = File.OpenWrite(filename);
        await filestream.CopyToAsync(file);
        await file.FlushAsync();
        file.Close();
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(string filename, byte[] bytes)
    {
        await File.WriteAllBytesAsync(filename, bytes);
    }

    /// <inheritdoc />
    public Task<bool> FileExistsAsync(string filename)
    {
        var file = new FileInfo(filename);
        return Task.FromResult(file.Exists);
    }

    /// <inheritdoc />
    public Task DeleteFileAsync(string filename)
    {
        var file = new FileInfo(filename);
        if (file.Exists)
        {
            file.Delete();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteFilesAsync(IList<string> filenames)
    {
        foreach (var filename in filenames)
        {
            await DeleteFileAsync(filename);
        }
    }
}
