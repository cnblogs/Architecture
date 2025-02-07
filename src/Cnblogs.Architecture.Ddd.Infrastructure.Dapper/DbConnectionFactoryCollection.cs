namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

/// <summary>
///     数据库连接的工厂类集合。
/// </summary>
public class DbConnectionFactoryCollection
{
    private readonly Dictionary<string, Type> _factories = new();

    /// <summary>
    ///     添加数据库连接工厂。
    /// </summary>
    /// <param name="name">名称。</param>
    /// <param name="factory">工厂示例。</param>
    /// <exception cref="InvalidOperationException">Throw when <paramref name="name"/> already been registered with other factory.</exception>
    public void AddDbConnectionFactory(string name, Type factory)
    {
        if (_factories.TryGetValue(name, out var value))
        {
            throw new InvalidOperationException(
                $"The dapper context already configured with db connection factory: {value.Name}");
        }

        _factories.Add(name, factory);
    }

    /// <summary>
    ///     获取数据库工厂。
    /// </summary>
    /// <param name="name">名称。</param>
    /// <returns>工厂示例。</returns>
    public Type GetFactory(string name)
    {
        return _factories[name];
    }
}
