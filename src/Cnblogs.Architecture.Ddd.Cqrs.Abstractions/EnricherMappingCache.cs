using System.Collections.Concurrent;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Cache for enricher mapping, it should be singleton.
/// </summary>
public class EnricherMappingCache
{
    private readonly ConcurrentDictionary<Type, Type> _elementTypeCache = new();
}
