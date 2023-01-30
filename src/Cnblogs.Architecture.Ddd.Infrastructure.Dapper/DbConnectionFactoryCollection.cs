namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper;

/// <summary>
///     数据库连接的工厂类集合。
/// </summary>
public class DbConnectionFactoryCollection
{
    private readonly Dictionary<string, IDbConnectionFactory> _factories = new();

    /// <summary>
    ///     添加数据库连接工厂。
    /// </summary>
    /// <param name="name">名称。</param>
    /// <param name="factory">工厂示例。</param>
    public void AddDbConnectionFactory(string name, IDbConnectionFactory factory)
    {
        if (_factories.ContainsKey(name))
        {
            _factories[name] = factory;
            return;
        }

        _factories.Add(name, factory);
    }

    /// <summary>
    ///     获取数据库工厂。
    /// </summary>
    /// <param name="name">名称。</param>
    /// <returns>工厂示例。</returns>
    public IDbConnectionFactory GetFactory(string name)
    {
        return _factories[name];
    }
}