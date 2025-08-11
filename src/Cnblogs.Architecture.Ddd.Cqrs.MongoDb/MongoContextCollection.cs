using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

namespace Cnblogs.Architecture.Ddd.Cqrs.MongoDb;

/// <summary>
///     MongoContext 列表，用于初始化配置时遍历。
/// </summary>
public class MongoContextCollection
{
    private readonly List<Type> _contexts = [];

    /// <summary>
    ///     添加一个 MongoContext。
    /// </summary>
    /// <typeparam name="TContext">MongoContext 类型。</typeparam>
    public void Add<TContext>()
        where TContext : MongoContext
    {
        var type = typeof(TContext);
        if (_contexts.Contains(type) == false)
        {
            _contexts.Add(type);
        }
    }

    /// <summary>
    ///     获取注册的 MongoContext 列表。
    /// </summary>
    public IEnumerable<Type> MongoContexts => _contexts;
}