namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     Provides abstractions for accessing file system.
/// </summary>
public interface IFileProvider
{
    /// <summary>
    ///     Get file content by filename.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns>File's content stream.</returns>
    /// <exception cref="FileNotFoundException">Throw if file with filename does not exist.</exception>
    Task<Stream> GetFileStreamAsync(string filename);

    /// <summary>
    ///     Get file content by filename.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns>File's content in byte array.</returns>
    /// <exception cref="FileNotFoundException">Throw if file with filename does not exist.</exception>
    Task<byte[]> GetFileBytesAsync(string filename);

    /// <summary>
    ///     Save file to given filename.
    /// </summary>
    /// <param name="filename">The path to save file to.</param>
    /// <param name="filestream">The file content.</param>
    /// <returns></returns>
    Task SaveFileAsync(string filename, Stream filestream);

    /// <summary>
    ///     Save file to given filename.
    /// </summary>
    /// <param name="filename">The path to save file to.</param>
    /// <param name="bytes">The file content in byte array.</param>
    /// <returns></returns>
    Task SaveFileAsync(string filename, byte[] bytes);

    /// <summary>
    ///     Check if file exists.
    /// </summary>
    /// <param name="filename">The filename to check.</param>
    /// <returns>True if file exists.</returns>
    Task<bool> FileExistsAsync(string filename);

    /// <summary>
    ///     Delete file with certain filename.
    /// </summary>
    /// <param name="filename">The filename to delete.</param>
    /// <returns></returns>
    Task DeleteFileAsync(string filename);

    /// <summary>
    ///     Bulk delete files by filenames.
    /// </summary>
    /// <param name="filenames">The files to be deleted.</param>
    /// <returns></returns>
    Task DeleteFilesAsync(IList<string> filenames);
}
