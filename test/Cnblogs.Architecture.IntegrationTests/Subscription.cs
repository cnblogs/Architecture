namespace Cnblogs.Architecture.IntegrationTests;

/// <summary>
/// This class defines subscribe endpoint response for dapr
/// </summary>
internal class Subscription
{
    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pubsub name
    /// </summary>
    public string PubsubName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route
    /// </summary>
    public string Route { get; set; } = string.Empty;
}
