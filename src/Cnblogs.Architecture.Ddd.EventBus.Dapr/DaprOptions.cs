namespace Cnblogs.Architecture.Ddd.EventBus.Dapr;

/// <summary>
///     Dapr 配置。
/// </summary>
public class DaprOptions
{
    /// <summary>
    ///     应用名称。
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    ///     使用的管道名称。
    /// </summary>
    public static string PubSubName { get; set; } = "pubsub";

    /// <summary>
    ///     是否调用过 <c>app.MapSubscribeHandler()</c>
    /// </summary>
    internal static bool IsDaprSubscribeHandlerMapped { get; set; }
}