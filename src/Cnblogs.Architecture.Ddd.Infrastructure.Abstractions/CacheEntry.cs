namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     从缓存中取出的结果。
/// </summary>
/// <typeparam name="T">缓存值的类型。</typeparam>
/// <param name="Value">缓存值。</param>
/// <param name="TimestampInSeconds">缓存创建时间。</param>
public record CacheEntry<T>(T Value, long TimestampInSeconds);