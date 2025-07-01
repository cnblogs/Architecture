using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     管理可排序列的映射。
/// </summary>
public static class OrderBySegmentConfig
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, OrderBySegment>> Cache = new();

    /// <summary>
    ///     注册新的可排序列。
    /// </summary>
    /// <param name="name">列名。</param>
    /// <param name="exp">属性表达式。</param>
    /// <typeparam name="TSource">属性对应的实体。</typeparam>
    /// <typeparam name="TProperty">属性类型。</typeparam>
    public static void RegisterSortableProperty<TSource, TProperty>(
        string name,
        Expression<Func<TSource, TProperty>> exp)
    {
        var sourceType = typeof(TSource);
        if (Cache.ContainsKey(sourceType) == false)
        {
            Cache[sourceType] = new ConcurrentDictionary<string, OrderBySegment>(StringComparer.OrdinalIgnoreCase);
        }

        Cache[sourceType][name] = new OrderBySegment(false, exp);
        Cache[sourceType]["-" + name] = new OrderBySegment(true, exp);
    }

    private static List<string> SplitSortStrings(this string orderByString)
    {
        return orderByString
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    /// <summary>
    ///     解析 <see cref="OrderBySegment" />，不合法的列名将被忽略。
    /// </summary>
    /// <param name="input">输入字符串。</param>
    /// <param name="segments">解析出的 <see cref="OrderBySegment" />。</param>
    /// <typeparam name="T">排序对应的实体类型。</typeparam>
    /// <returns>是否解析成功。</returns>
    public static bool TryParseOrderBySegments<T>(
        string? input,
        [NotNullWhen(true)] out List<OrderBySegment>? segments)
    {
        segments = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (Cache.TryGetValue(typeof(T), out var typeCache) == false)
        {
            return false;
        }

        segments = new List<OrderBySegment>();
        var segmentStrings = SplitSortStrings(input);
        foreach (var s in segmentStrings)
        {
            if (typeCache != null && typeCache.TryGetValue(s, out var segment))
            {
                segments.Add(segment);
            }
        }

        return segments.Count > 0;
    }
}

/// <summary>
///     管理可排序列的映射。
/// </summary>
/// <typeparam name="TEntity">The entity to config.</typeparam>
public static class OrderBySegmentConfig<TEntity>
{
    /// <summary>
    ///     注册新的可排序列。
    /// </summary>
    /// <param name="name">列名。</param>
    /// <param name="exp">属性表达式。</param>
    /// <typeparam name="TProperty">属性类型。</typeparam>
    public static void RegisterSortableProperty<TProperty>(
        string name,
        Expression<Func<TEntity, TProperty>> exp)
    {
        OrderBySegmentConfig.RegisterSortableProperty(name, exp);
    }
}
