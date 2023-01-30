namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     项目的 AppName，用于事件订阅路径。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyAppNameAttribute : Attribute
{
    /// <inheritdoc />
    public AssemblyAppNameAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     项目的 AppName
    /// </summary>
    public string Name { get; }
}