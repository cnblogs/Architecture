using Cuiliang.AliyunOssSdk.Entites;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     The aliyun oss options.
/// </summary>
public class AliyunOssOptions
{
    private BucketInfo? _bucketInfo;

    /// <summary>
    ///     OSS access key id.
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    ///     OSS access key secret.
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    ///     OSS security token.
    /// </summary>
    public string SecurityToken { get; set; } = string.Empty;

    /// <summary>
    ///     The bucket name.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    ///     The region that bucket belongs to.
    /// </summary>
    public string Region { get; set; } = OssRegions.HangZhou;

    /// <summary>
    ///     True if HTTPS is enabled.
    /// </summary>
    public bool UseHttps { get; set; }

    /// <summary>
    ///     True if OSS is used by internal resources.
    /// </summary>
    public bool UseInternal { get; set; }

    /// <summary>
    ///     The bucket info of OSS.
    /// </summary>
    public BucketInfo BucketInfo
        => _bucketInfo ??= BucketInfo.CreateByRegion(Region, BucketName, UseHttps, UseInternal);
}
