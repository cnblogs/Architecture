using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Models;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     Aliyun OSS implementation of <see cref="IFileDeliveryProvider"/>.
/// </summary>
/// <param name="client">The oss client.</param>
/// <param name="dateTimeProvider">Datetime provider for expiration calculation.</param>
/// <param name="options">The options for oss client.</param>
public class AliyunOssFileDeliveryProvider(
    Client client,
    IDateTimeProvider dateTimeProvider,
    IOptions<AliyunOssOptions> options)
    : IFileDeliveryProvider
{
    private readonly AliyunOssOptions _options = options.Value;

    /// <inheritdoc />
    public Task<string> GetDownloadUrlAsync(string filename, TimeSpan duration)
    {
        var link = client.Presign(
            new GetObjectRequest
            {
                Bucket = _options.BucketName, Key = filename,
            },
            dateTimeProvider.Now().DateTime.Add(duration));

        if (string.IsNullOrEmpty(link.Url))
        {
            throw new InvalidOperationException(
                $"[{nameof(AliyunOssFileDeliveryProvider)}] Generate presigned link failed for {filename}");
        }

        return Task.FromResult(link.Url);
    }
}
