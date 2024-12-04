namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     定义 UnitOfWork。
/// </summary>
/// <typeparam name="TEntity">实体类型。</typeparam>
/// <typeparam name="TKey">主键类型。</typeparam>
public interface IUnitOfWork<TEntity, TKey>
    where TEntity : EntityBase, IAggregateRoot
    where TKey : IComparable<TKey>
{
    /// <summary>
    ///     获取实体以供更改。
    /// </summary>
    /// <param name="key">主键。</param>
    /// <returns>获取到的实体。</returns>
    Task<TEntity?> GetAsync(TKey key);

    /// <summary>
    ///     添加实体，调用 <see cref="SaveEntitiesAsync"/> 或 <see cref="SaveChangesAsync"/> 后才会写入数据库。
    /// </summary>
    /// <param name="entity">要添加实体。</param>
    /// <returns>被添加的实体。</returns>
    TEntity Add(TEntity entity);

    /// <summary>
    ///     更新实体，调用 <see cref="SaveEntitiesAsync"/> 或 <see cref="SaveChangesAsync"/> 后才会写入数据库。
    /// </summary>
    /// <param name="entity">要更新的实体。</param>
    /// <returns>被更新的实体。</returns>
    TEntity Update(TEntity entity);

    /// <summary>
    ///     删除实体，调用 <see cref="SaveEntitiesAsync"/> 或 <see cref="SaveChangesAsync"/> 后才会写入数据库。
    /// </summary>
    /// <param name="entity">要删除的实体。</param>
    /// <returns></returns>
    TEntity Delete(TEntity entity);

    /// <summary>
    ///     提交所有更改（但不发布领域事件）。
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken" />。</param>
    /// <returns>更改的行数。</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     提交所有实体，会发送领域事件（如果有）。
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken" />。</param>
    /// <returns>提交是否成功。</returns>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
