using AlibabaCloud.OSS.V2.Models;

namespace Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss;

/// <summary>
///     The aliyun oss options.
/// </summary>
public class AliyunOssOptions
{
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
    public string? SecurityToken { get; set; }

    /// <summary>
    ///     The bucket name.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    ///     The region that bucket belongs to.
    /// </summary>
    public string Region { get; set; } = "cn-hangzhou";

    /// <summary>
    ///     True if HTTPS is enabled.
    /// </summary>
    public bool UseHttps { get; set; }

    /// <summary>
    ///     Custom OSS endpoint.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    ///     True if OSS is used by internal resources.
    /// </summary>
    public bool UseInternalEndpoint { get; set; }

    /// <summary>
    ///     True if you want to use OSS accelerate endpoint.
    /// </summary>
    public bool UseAccelerateEndpoint { get; set; }
}
