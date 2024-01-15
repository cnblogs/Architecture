using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cuiliang.AliyunOssSdk;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     Aliyun OSS implementation of <see cref="IFileDeliveryProvider"/>.
/// </summary>
public class AliyunOssFileDeliveryProvider : IFileDeliveryProvider
{
    private readonly OssClient _client;
    private readonly AliyunOssOptions _options;

    /// <summary>
    ///     Create a <see cref="AliyunOssFileDeliveryProvider"/>.
    /// </summary>
    /// <param name="client">The oss client.</param>
    /// <param name="options">The options for oss client.</param>
    public AliyunOssFileDeliveryProvider(OssClient client, IOptions<AliyunOssOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<string> GetDownloadUrlAsync(string filename, TimeSpan duration)
    {
        var meta = await _client.GetObjectMetaAsync(_options.BucketInfo, filename);
        if (meta.IsSuccess == false)
        {
            throw new FileNotFoundException(meta.ErrorMessage, filename, meta.InnerException);
        }

        return _client.GetFileDownloadLink(
            _options.BucketInfo,
            filename,
            (int)Math.Ceiling(duration.TotalSeconds));
    }
}
