namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     Repository that support raw sql execution.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
/// <typeparam name="TKey">The type of key.</typeparam>
public interface ISqlRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : EntityBase, IAggregateRoot
    where TKey : IComparable<TKey>
{
    /// <summary>
    ///     Query entity with raw sql.
    /// </summary>
    /// <param name="sql">The sql string.</param>
    /// <param name="parameters">The parameters</param>
    /// <returns></returns>
    IQueryable<TEntity> SqlQuery(string sql, params object[] parameters);

    /// <summary>
    ///     Query with raw sql.
    /// </summary>
    /// <param name="sql">The sql string.</param>
    /// <param name="parameters">The parameters.</param>
    /// <typeparam name="T">The type of query result.</typeparam>
    /// <returns></returns>
    IQueryable<T> SqlQuery<T>(string sql, params object[] parameters);

    /// <summary>
    ///     Execute raw sql.
    /// </summary>
    /// <param name="sql">The sql string.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
}
