using MongoDB.Driver.Linq;

namespace Cnblogs.Architecture.Ddd.Cqrs.MongoDb;

/// <summary>
///     MongoDb Queryable 扩展方法。
/// </summary>
public static class MongoQueryableExtensions
{
    /// <summary>
    ///     将 <see cref="IQueryable{T}" /> 转换为 <see cref="IMongoQueryable{T}" />。
    /// </summary>
    /// <param name="queryable">输入的 IQueryable。</param>
    /// <typeparam name="T">查询类型。</typeparam>
    /// <returns>转换后的 <see cref="IMongoQueryable{T}" /></returns>
    /// <exception cref="InvalidCastException">输入的 <see cref="IQueryable{T}" /> 不是 <see cref="IMongoQueryable{T}" />。</exception>
    public static IMongoQueryable<T> AsMongoQueryable<T>(this IQueryable<T> queryable)
    {
        if (queryable is IMongoQueryable<T> mongoQueryable)
        {
            return mongoQueryable;
        }

        throw new InvalidCastException("input is not mongo queryable");
    }
}