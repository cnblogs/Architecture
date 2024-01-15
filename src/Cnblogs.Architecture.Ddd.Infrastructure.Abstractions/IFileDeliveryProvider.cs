namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     File provider that can create public url for user to download
/// </summary>
public interface IFileDeliveryProvider
{
    /// <summary>
    ///     Get public url to download with validate time.
    /// </summary>
    /// <param name="filename">The file filename.</param>
    /// <param name="duration">Duration of url availability.</param>
    /// <returns></returns>
    public Task<string> GetDownloadUrlAsync(string filename, TimeSpan duration);
}
