namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     定义仓储类。
/// </summary>
/// <typeparam name="TEntity">仓储负责的实体类型。</typeparam>
/// <typeparam name="TKey"><typeparamref name="TEntity" /> 的主键类型。</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : EntityBase, IAggregateRoot
    where TKey : IComparable<TKey>
{
    /// <summary>
    ///     获取用于查询的 <see cref="IQueryable{T}" />。
    /// </summary>
    IQueryable<TEntity> NoTrackingQueryable { get; }

    /// <summary>
    ///     获取一个 <see cref="IUnitOfWork{TEntity,TKey}"/>。
    /// </summary>
    IUnitOfWork<TEntity, TKey> UnitOfWork { get; }

    /// <summary>
    ///     添加一个新的实体并立即提交。
    /// </summary>
    /// <param name="entity">要添加的实体。</param>
    /// <returns>添加后的实体。</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    ///     添加多个实体并立即提交。
    /// </summary>
    /// <typeparam name="TEnumerable">列表类型。</typeparam>
    /// <param name="entities">要添加的实体。</param>
    /// <returns>添加后的实体。</returns>
    Task<TEnumerable> AddRangeAsync<TEnumerable>(TEnumerable entities)
        where TEnumerable : IEnumerable<TEntity>;

    /// <summary>
    ///     删除实体并立即提交。
    /// </summary>
    /// <param name="entity">要删除的实体。</param>
    /// <returns>被删除的实体。</returns>
    Task<TEntity> DeleteAsync(TEntity entity);

    /// <summary>
    ///     通过主键获取实体以供修改。
    /// </summary>
    /// <param name="key">实体主键。</param>
    /// <returns><paramref name="key" /> 对应的实体。</returns>
    Task<TEntity?> GetAsync(TKey key);

    /// <summary>
    ///     获取用于查询的 <see cref="IQueryable{T}" />
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <returns>NoTrackingQueryable</returns>
    IQueryable<T> GetNoTrackingQueryable<T>()
        where T : class;

    /// <summary>
    ///     更新实体并立即提交。
    /// </summary>
    /// <param name="entity">要更新的实体。</param>
    /// <returns>更新后的实体。</returns>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    ///     批量更新实体并提交。
    /// </summary>
    /// <param name="entities">实体。</param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);
}