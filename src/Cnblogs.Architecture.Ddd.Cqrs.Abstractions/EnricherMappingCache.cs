using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions.Internals;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Cache for enricher mapping, it should be singleton.
/// </summary>
public class EnricherMappingCache
{
    private readonly ConcurrentDictionary<Type, ContainerInfo> _containerInfoCache = new();
    private readonly ConcurrentDictionary<Type, List<EnricherStage>> _enricherPlans = new();

    /// <summary>
    ///     Get container info for a type, caching the result.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    public ContainerInfo GetContainerInfo(Type type)
    {
        return _containerInfoCache.GetOrAdd(type, ResolveContainerInfo);
    }

    /// <summary>
    ///     Build enrich plan with given <paramref name="targetType"/> and <paramref name="enricherTypes"/>
    /// </summary>
    /// <param name="targetType">The element type to enrich.</param>
    /// <param name="enricherTypes">Types of enrichers.</param>
    public void BuildEnrichPlan(Type targetType, ICollection<Type> enricherTypes)
    {
        // ReSharper disable once HeapView.CanAvoidClosure
        _enricherPlans.GetOrAdd(targetType, _ => CompileEnricherPlan(enricherTypes));
    }

    internal List<EnricherStage>? GetEnricherPlan(Type elementType)
    {
        var success = _enricherPlans.TryGetValue(elementType, out var list);
        return success ? list : null;
    }

    private static List<EnricherStage> CompileEnricherPlan(ICollection<Type> enricherTypes)
    {
        if (enricherTypes.Count == 0)
        {
            return [];
        }

        var typeSet = enricherTypes.ToHashSet();
        var descriptors = typeSet.ToDictionary(t => t, CreateDescriptor);
        var inDegree = typeSet.ToDictionary(x => x, _ => 0);
        var adjacencyList = typeSet.ToDictionary(x => x, _ => new List<Type>());

        // Resolve the enricher interface for type checking
        var elementType = descriptors.Values.First().EnrichMethod.GetParameters()[0].ParameterType;
        var enricherType = typeof(IEnricher<>).MakeGenericType(elementType);

        // Build edges from EnrichAfterAttribute
        foreach (var type in typeSet)
        {
            var attrs = type.GetCustomAttributes<EnrichAfterAttribute>();
            foreach (var dep in attrs.SelectMany(x => x.DependencyTypes).Where(x => x.IsAssignableTo(enricherType)))
            {
                if (typeSet.Contains(dep))
                {
                    // Edge: dep -> type (dep must run BEFORE type)
                    adjacencyList[dep].Add(type);
                    inDegree[type]++;
                    continue;
                }

                // Dependency is not in the same group, this is a configuration error
                throw new InvalidOperationException(
                    $"Enricher '{type}' depends on '{dep}', but '{dep}' is not registered/resolved for this element type.");
            }
        }

        return BuildDag(typeSet, descriptors, inDegree, adjacencyList);
    }

    private static EnricherDescriptor CreateDescriptor(Type enricherType)
    {
        var enricherInterface = enricherType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnricher<>));
        return new EnricherDescriptor(
            enricherType,
            enricherInterface.GetMethod("EnrichAsync")!,
            enricherInterface.GetMethod("BulkEnrichAsync")!);
    }

    private static List<EnricherStage> BuildDag(
        HashSet<Type> enricherTypes,
        Dictionary<Type, EnricherDescriptor> descriptors,
        Dictionary<Type, int> inDegree,
        Dictionary<Type, List<Type>> adjacencyList)
    {
        var stages = new List<EnricherStage>();
        var currentStageNodes = inDegree.Where(x => x.Value == 0).Select(x => x.Key).ToList();
        var processedCount = 0;

        // Process level by level
        while (currentStageNodes.Count > 0)
        {
            stages.Add(new EnricherStage(currentStageNodes.Select(t => descriptors[t]).ToList()));
            processedCount += currentStageNodes.Count;

            var nextStageNodes = new List<Type>();
            foreach (var dependent in currentStageNodes.SelectMany(node => adjacencyList[node]))
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                {
                    nextStageNodes.Add(dependent);
                }
            }

            currentStageNodes = nextStageNodes;
        }

        // Circular dependency check
        return processedCount == enricherTypes.Count
            ? stages
            : throw new InvalidOperationException("Circular dependency detected among enrichers.");
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
