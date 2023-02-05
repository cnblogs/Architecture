namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     项目的 AppName，用于事件订阅路径。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyAppNameAttribute : Attribute
{
    /// <summary>
    ///     配置应用名称。
    /// </summary>
    /// <param name="name">应用名称。</param>
    public AssemblyAppNameAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     项目的 AppName
    /// </summary>
    public string Name { get; }
}