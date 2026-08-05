using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Models;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     An <see cref="IFileProvider"/> implementation using Aliyun OSS.
/// </summary>
/// <param name="ossClient">The underlying Aliyun OSS client.</param>
/// <param name="options">The Aliyun OSS options.</param>
public class AliyunOssFileProvider(Client ossClient, IOptions<AliyunOssOptions> options) : IFileProvider
{
    private readonly AliyunOssOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<Stream> GetFileStreamAsync(string filename)
    {
        var file = await ossClient.GetObjectAsync(
            new GetObjectRequest { Bucket = _options.BucketName, Key = filename });
        if (file.StatusCode == 404)
        {
            throw new FileNotFoundException(filename);
        }

        return file.Body ?? throw new FileNotFoundException(filename);
    }

    /// <inheritdoc />
    public async Task<byte[]> GetFileBytesAsync(string filename)
    {
        await using var stream = await GetFileStreamAsync(filename);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(string filename, Stream filestream)
    {
        var response = await ossClient.PutObjectAsync(
            new PutObjectRequest()
            {
                Bucket = _options.BucketName,
                Key = filename,
                Body = filestream
            });
        if (response.StatusCode >= 400)
        {
            throw new InvalidOperationException(
                $"[{nameof(AliyunOssFileProvider)}] Save file failed, fileName: {filename}, statusCode: {response.StatusCode}, requestId: {response.RequestId}, message: {response.Status}");
        }
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(string filename, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        await SaveFileAsync(filename, stream);
    }

    /// <inheritdoc />
    public Task<bool> FileExistsAsync(string filename)
    {
        return ossClient.IsObjectExistAsync(_options.BucketName, filename);
    }

    /// <inheritdoc />
    public async Task DeleteFilesAsync(IList<string> filenames)
    {
        var requests = filenames.Select(x => new DeleteObject() { Key = x }).ToList();
        var response = await ossClient.DeleteMultipleObjectsAsync(
            new DeleteMultipleObjectsRequest() { Bucket = _options.BucketName, Objects = requests });
        if (response.StatusCode >= 400)
        {
            throw new InvalidOperationException(
                $"[{nameof(AliyunOssFileProvider)}] Delete files failed, statusCode: {response.StatusCode}, requestId: {response.RequestId}, message: {response.Status}");
        }
    }

    /// <inheritdoc />
    public async Task DeleteFileAsync(string filename)
    {
        var response = await ossClient.DeleteObjectAsync(
            new DeleteObjectRequest { Bucket = _options.BucketName, Key = filename });
        if (response.StatusCode >= 400)
        {
            throw new InvalidOperationException(
                $"[{nameof(AliyunOssFileProvider)}] Delete file failed, fileName: {filename}, statusCode: {response.StatusCode}, requestId: {response.RequestId}, message: {response.Status}");
        }
    }

    private static FileNotFoundException NewFileNotFoundException(string path)
    {
        return new FileNotFoundException($"[{nameof(AliyunOssFileProvider)}] File not found", path);
    }
}
