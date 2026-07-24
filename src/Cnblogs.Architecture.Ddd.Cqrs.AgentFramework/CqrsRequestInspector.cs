using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Detects whether a type is a CQRS Command or Query (and whether it is pageable) by inspecting its closed generic interfaces.
/// </summary>
internal static class CqrsRequestInspector
{
    private static readonly Type[] CommandInterfaces =
    [
        typeof(ICommand<>),
        typeof(ICommand<,>),
    ];

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
    ///     Returns the <see cref="RequestKind" /> for <paramref name="type" />, or <see cref="RequestKind.None" /> if it is not a Command or Query.
    /// </summary>
    public static RequestKind GetKind(Type type)
    {
        var definitions = type.GetInterfaces()
            .Where(i => i.IsGenericType)
            .Select(i => i.GetGenericTypeDefinition())
            .ToHashSet();

        if (CommandInterfaces.Any(d => definitions.Contains(d)))
        {
            return RequestKind.Command;
        }

        return QueryInterfaces.Any(d => definitions.Contains(d)) ? RequestKind.Query : RequestKind.None;
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
