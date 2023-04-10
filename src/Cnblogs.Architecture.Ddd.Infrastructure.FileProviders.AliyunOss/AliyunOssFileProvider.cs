using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cuiliang.AliyunOssSdk;
using Cuiliang.AliyunOssSdk.Api;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     An <see cref="IFileProvider"/> implementation using Aliyun OSS.
/// </summary>
public class AliyunOssFileProvider : IFileProvider
{
    private readonly OssClient _ossClient;
    private readonly AliyunOssOptions _options;

    /// <summary>
    ///     Create a <see cref="IFileProvider"/> based on Aliyun OSS.
    /// </summary>
    /// <param name="ossClient">The underlying Aliyun OSS client.</param>
    /// <param name="options">The Aliyun OSS options.</param>
    public AliyunOssFileProvider(OssClient ossClient, IOptions<AliyunOssOptions> options)
    {
        _ossClient = ossClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Stream> GetFileStreamAsync(string filename)
    {
        var file = await _ossClient.GetObjectAsync(_options.BucketInfo, filename);
        if (file.IsSuccess == false)
        {
            throw NewFileNotFoundException(filename, file);
        }

        return await file.SuccessResult.Content.ReadAsStreamAsync();
    }

    /// <inheritdoc />
    public async Task<byte[]> GetFileBytesAsync(string filename)
    {
        var file = await _ossClient.GetObjectAsync(_options.BucketInfo, filename);
        if (file.IsSuccess == false)
        {
            throw NewFileNotFoundException(filename, file);
        }

        return await file.SuccessResult.Content.ReadAsByteArrayAsync();
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(string filename, Stream filestream)
    {
        var result = await _ossClient.PutObjectAsync(_options.BucketInfo, filename, filestream);
        if (result.IsSuccess == false)
        {
            throw new InvalidOperationException(result.ErrorMessage, result.InnerException);
        }
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(string filename, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        await SaveFileAsync(filename, stream);
    }

    /// <inheritdoc />
    public async Task<bool> FileExistsAsync(string filename)
    {
        var result = await _ossClient.GetObjectMetaAsync(_options.BucketInfo, filename);
        return result.IsSuccess;
    }

    /// <inheritdoc />
    public async Task DeleteFilesAsync(IList<string> filenames)
    {
        await _ossClient.DeleteMultipleObjectsAsync(_options.BucketInfo, filenames, true);
    }

    /// <inheritdoc />
    public async Task DeleteFileAsync(string filename)
    {
        await _ossClient.DeleteObjectAsync(_options.BucketInfo, filename);
    }

    private static FileNotFoundException NewFileNotFoundException<T>(string path, OssResult<T> result)
    {
        return new FileNotFoundException(result.ErrorMessage, path, result.InnerException);
    }
}
