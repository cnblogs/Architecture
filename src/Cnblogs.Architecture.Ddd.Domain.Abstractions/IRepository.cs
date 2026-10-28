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
    ///     删除实体并立即提交。默认软删除，即把 <see cref="Entity.Deleted" /> 置为 <c>true</c>。
    /// </summary>
    /// <param name="entity">要删除的实体。</param>
    /// <param name="softDelete">是否软删除，默认 <c>true</c>；软删除要求实体继承 <see cref="Entity" />。</param>
    /// <returns>被删除的实体。</returns>
    /// <exception cref="InvalidOperationException"><paramref name="softDelete" /> 为 <c>true</c> 且实体未继承 <see cref="Entity" />。</exception>
    Task<TEntity> DeleteAsync(TEntity entity, bool softDelete = true);

    /// <summary>
    ///     批量删除实体并立即提交。默认软删除，即把 <see cref="Entity.Deleted" /> 置为 <c>true</c>。
    /// </summary>
    /// <param name="entities">要删除的实体。</param>
    /// <param name="softDelete">是否软删除，默认 <c>true</c>；软删除要求实体继承 <see cref="Entity" />。</param>
    /// <returns>被删除的实体。</returns>
    /// <exception cref="InvalidOperationException"><paramref name="softDelete" /> 为 <c>true</c> 且任一实体未继承 <see cref="Entity" />。</exception>
    Task<List<TEntity>> DeleteRangeAsync(IEnumerable<TEntity> entities, bool softDelete = true);

    /// <summary>
    ///     通过主键获取实体以供修改。
    /// </summary>
    /// <param name="key">实体主键。</param>
    /// <returns><paramref name="key" /> 对应的实体。</returns>
    Task<TEntity?> GetAsync(TKey key);

    /// <summary>
    ///     通过主键批量获取实体以供修改。
    /// </summary>
    /// <param name="keys">实体主键列表。</param>
    /// <returns>主键匹配的实体列表，不含未找到的主键对应的项。</returns>
    Task<List<TEntity>> GetRangeAsync(IEnumerable<TKey> keys);

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
