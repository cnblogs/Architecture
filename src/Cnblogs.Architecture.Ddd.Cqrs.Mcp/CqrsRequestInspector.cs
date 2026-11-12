using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Mcp;

/// <summary>
///     Detects whether a type is a CQRS Command or Query (and whether it is pageable) by inspecting its closed generic
///     interfaces.
/// </summary>
internal static class CqrsRequestInspector
{
    private static readonly Type[] QueryInterfaces =
    [
        typeof(IQuery<>),
        typeof(IListQuery<>),
        typeof(IOrderedQuery<>),
        typeof(IPageableQuery<>),
        typeof(IModelQuery<>),
        typeof(IPageableModelQuery<>),
    ];

    /// <summary>
    ///     Whether <paramref name="type" /> implements an <c>IQuery&lt;T&gt;</c>-family interface.
    /// </summary>
    public static bool IsQuery(Type type)
    {
        var definitions = type.GetInterfaces()
            .Where(i => i.IsGenericType)
            .Select(i => i.GetGenericTypeDefinition())
            .ToHashSet();
        return QueryInterfaces.Any(d => definitions.Contains(d));
    }

    /// <summary>
    ///     Whether <paramref name="type" /> is an <c>IPageableQuery&lt;TElement&gt;</c> or <c>IPageableModelQuery&lt;TModel&gt;</c>.
    /// </summary>
    public static bool IsPageable(Type type)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType
            && (i.GetGenericTypeDefinition() == typeof(IPageableQuery<>)
                || i.GetGenericTypeDefinition() == typeof(IPageableModelQuery<>)));
    }
}
