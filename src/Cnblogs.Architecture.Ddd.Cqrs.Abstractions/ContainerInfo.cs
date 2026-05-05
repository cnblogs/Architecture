using System.Collections;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

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
    ///     Whether the type is an enumerable container.
    /// </summary>
    public bool IsEnumerable { get; }

    /// <summary>
    ///     Extracts items from a container instance. Null when <see cref="IsEnumerable" /> is false.
    /// </summary>
    public Func<object, IEnumerable>? ExtractItems { get; }

    /// <summary>
    ///     Creates a new <see cref="ContainerInfo" /> for an enumerable container.
    /// </summary>
    /// <param name="elementType">The element type contained in the container.</param>
    /// <param name="extractItems">A function that extracts items from a container instance.</param>
    public ContainerInfo(Type elementType, Func<object, IEnumerable> extractItems)
    {
        ElementType = elementType;
        IsEnumerable = true;
        ExtractItems = extractItems;
    }

    /// <summary>
    ///     Creates a new <see cref="ContainerInfo" /> for a non-enumerable type.
    /// </summary>
    /// <param name="elementType">The type itself.</param>
    public ContainerInfo(Type elementType)
    {
        ElementType = elementType;
        IsEnumerable = false;
    }
}
