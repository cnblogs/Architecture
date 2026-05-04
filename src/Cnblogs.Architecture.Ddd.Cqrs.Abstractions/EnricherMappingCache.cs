using System.Collections;
using System.Collections.Concurrent;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Cache for enricher mapping, it should be singleton.
/// </summary>
public class EnricherMappingCache
{
    private readonly ConcurrentDictionary<Type, ContainerInfo?> _containerInfoCache = new();

    /// <summary>
    ///     Get container info for a type, caching the result.
    ///     Returns null if the type is not a recognized container.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    public ContainerInfo? GetContainerInfo(Type type)
    {
        return _containerInfoCache.GetOrAdd(type, ResolveContainerInfo);
    }

    private static ContainerInfo? ResolveContainerInfo(Type t)
    {
        // IPagedList<T>
        if (typeof(IPagedList).IsAssignableFrom(t) && t.IsGenericType)
        {
            var args = t.GetGenericArguments();
            if (args.Length > 0)
            {
                return new ContainerInfo(args[0], obj => ((IPagedList)obj).GetItems());
            }
        }

        // IDictionary<K,V> (Dictionary<K,V>, ConcurrentDictionary<K,V>, SortedDictionary<K,V>, etc.)
        var dictInterface = t.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        if (dictInterface is not null)
        {
            return new ContainerInfo(
                dictInterface.GetGenericArguments()[1],
                obj => ((IDictionary)obj).Values);
        }

        // IReadOnlyDictionary<K,V> (ReadOnlyDictionary<K,V>, FrozenDictionary<K,V>, etc.)
        var readOnlyDictInterface = t.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
        if (readOnlyDictInterface is not null)
        {
            var valuesProp = t.GetProperty("Values")!;
            return new ContainerInfo(
                readOnlyDictInterface.GetGenericArguments()[1],
                obj => (IEnumerable)valuesProp.GetValue(obj)!);
        }

        if (t == typeof(string))
        {
            return null;
        }

        // IEnumerable<T> (List<T>, T[], etc.)
        var enumInterface = t.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (enumInterface is not null)
        {
            return new ContainerInfo(
                enumInterface.GetGenericArguments()[0],
                obj => (IEnumerable)obj);
        }

        return null;
    }
}

/// <summary>
///     Describes how to extract items from a container type.
/// </summary>
public sealed class ContainerInfo
{
    /// <summary>
    ///     The element type contained in the container.
    /// </summary>
    public Type ElementType { get; }

    /// <summary>
    ///     Extracts items from a container instance.
    /// </summary>
    public Func<object, IEnumerable> ExtractItems { get; }

    /// <summary>
    ///     Creates a new <see cref="ContainerInfo" />.
    /// </summary>
    /// <param name="elementType">The element type contained in the container.</param>
    /// <param name="extractItems">A function that extracts items from a container instance.</param>
    public ContainerInfo(Type elementType, Func<object, IEnumerable> extractItems)
    {
        ElementType = elementType;
        ExtractItems = extractItems;
    }
}
