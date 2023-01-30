namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     缓存提供器。
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    ///     添加 <paramref name="value" /> 到缓存。
    /// </summary>
    /// <param name="cacheKey">缓存键。</param>
    /// <param name="value">缓存值。</param>
    /// <typeparam name="TResult">缓存类型。</typeparam>
    /// <returns>添加是否成功。</returns>
    Task<bool> AddAsync<TResult>(string cacheKey, TResult value);

    /// <summary>
    ///     添加 <paramref name="value" /> 到缓存，并设置过期时间。
    /// </summary>
    /// <param name="cacheKey">缓存键。</param>
    /// <param name="expires">过期时间。</param>
    /// <param name="value">缓存值。</param>
    /// <typeparam name="TResult">缓存类型。</typeparam>
    /// <returns>添加是否成功。</returns>
    Task<bool> AddAsync<TResult>(string cacheKey, TimeSpan expires, TResult value);

    /// <summary>
    ///     从缓存中获取 <paramref name="cacheKey" /> 对应的值。
    /// </summary>
    /// <param name="cacheKey">缓存键。</param>
    /// <typeparam name="TResult">缓存值类型。</typeparam>
    /// <returns>获取的缓存结果。</returns>
    Task<CacheEntry<TResult>?> GetAsync<TResult>(string cacheKey);

    /// <summary>
    ///     删除 <paramref name="cacheKey" /> 对应的缓存。
    /// </summary>
    /// <param name="cacheKey">缓存键。</param>
    /// <returns>删除是否成功。</returns>
    Task<bool> RemoveAsync(string cacheKey);

    /// <summary>
    ///     更新 <paramref name="cacheKey" /> 对应的缓存。
    /// </summary>
    /// <param name="cacheKey">缓存键。</param>
    /// <param name="value">缓存值。</param>
    /// <typeparam name="TResult">缓存值的类型。</typeparam>
    /// <returns>更新是否成功。</returns>
    Task<bool> UpdateAsync<TResult>(string cacheKey, TResult value);

    /// <summary>
    ///     更新 <paramref name="cacheKey" /> 对应的缓存。
    /// </summary>
    /// <param name="cacheKey">缓存键。</param>
    /// <param name="value">缓存值。</param>
    /// <param name="expires">缓存过期时间。</param>
    /// <typeparam name="TResult">缓存值的类型。</typeparam>
    /// <returns>更新是否成功。</returns>
    Task<bool> UpdateAsync<TResult>(string cacheKey, TResult value, TimeSpan expires);
}