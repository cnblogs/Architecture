using System.Collections;
using System.Collections.Concurrent;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Cache for enricher mapping, it should be singleton.
/// </summary>
public class EnricherMappingCache
{
    private readonly ConcurrentDictionary<Type, ContainerInfo> _containerInfoCache = new();
    private readonly ConcurrentDictionary<Type, EnricherTypeInfo> _enricherTypeInfoCache = new();

    /// <summary>
    ///     Get container info for a type, caching the result.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    public ContainerInfo GetContainerInfo(Type type)
    {
        return _containerInfoCache.GetOrAdd(type, ResolveContainerInfo);
    }

    /// <summary>
    ///     Get enricher type info for an element type, caching the result.
    /// </summary>
    /// <param name="elementType">The element type to resolve enrichers for.</param>
    public EnricherTypeInfo GetEnricherTypeInfo(Type elementType)
    {
        return _enricherTypeInfoCache.GetOrAdd(elementType, static t => new EnricherTypeInfo(t));
    }

    private static ContainerInfo ResolveContainerInfo(Type t)
    {
        if (t == typeof(string))
        {
            return new ContainerInfo(t);
        }

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
        var interfaces = t.GetInterfaces();
        var dictInterface = GetGenericInterfaceType(typeof(IDictionary<,>));
        if (dictInterface is not null)
        {
            return new ContainerInfo(
                dictInterface.GetGenericArguments()[1],
                obj => ((IDictionary)obj).Values);
        }

        // IReadOnlyDictionary<K,V> (ReadOnlyDictionary<K,V>, FrozenDictionary<K,V>, etc.)
        var readOnlyDictInterface = GetGenericInterfaceType(typeof(IReadOnlyDictionary<,>));
        if (readOnlyDictInterface is not null)
        {
            var valuesProp = t.GetProperty("Values")!;
            return new ContainerInfo(
                readOnlyDictInterface.GetGenericArguments()[1],
                obj => (IEnumerable)valuesProp.GetValue(obj)!);
        }

        // IEnumerable<T> (List<T>, T[], etc.)
        var enumInterface = GetGenericInterfaceType(typeof(IEnumerable<>));
        if (enumInterface is not null)
        {
            return new ContainerInfo(
                enumInterface.GetGenericArguments()[0],
                obj => (IEnumerable)obj);
        }

        return new ContainerInfo(t);

        Type? GetGenericInterfaceType(Type genericInterfaceType)
        {
            return interfaces
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);
        }
    }
}
