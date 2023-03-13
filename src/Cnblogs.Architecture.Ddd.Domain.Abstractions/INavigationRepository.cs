using System.Linq.Expressions;

namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     支持导航属性的仓储类型。
/// </summary>
/// <typeparam name="TEntity">实体类型。</typeparam>
/// <typeparam name="TKey">主键类型。</typeparam>
public interface INavigationRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : EntityBase, IAggregateRoot
    where TKey : IComparable<TKey>
{
    /// <summary>
    ///     通过主键获取实体以供修改。
    /// </summary>
    /// <param name="key">实体主键。</param>
    /// <param name="includes">要额外加载的其他实体。</param>
    /// <returns><paramref name="key" /> 对应的实体。</returns>
    Task<TEntity?> GetAsync(TKey key, params Expression<Func<TEntity, object?>>[] includes);

    /// <summary>
    ///     Get entity by key.
    /// </summary>
    /// <param name="key">The key of entity.</param>
    /// <param name="includes">Include strings.</param>
    /// <returns>The entity with key equals to <paramref name="key"/>.</returns>
    Task<TEntity?> GetAsync(TKey key, params string[] includes);
}
