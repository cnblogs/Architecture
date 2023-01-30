namespace Cnblogs.Architecture.Ddd.EventBus.Dapr;

internal static class DaprUtils
{
    public static string GetDaprTopicName<TEvent>(string appName)
    {
        return GetDaprTopicName(appName, typeof(TEvent).Name);
    }

    public static string GetDaprTopicName(string appName, string eventName) => $"{appName}.{eventName}";
}