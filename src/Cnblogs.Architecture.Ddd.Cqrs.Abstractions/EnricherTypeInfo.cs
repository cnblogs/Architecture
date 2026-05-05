using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Cached reflection info for resolving and invoking enrichers for a type.
/// </summary>
public sealed class EnricherTypeInfo
{
    /// <summary>
    ///     The closed <see cref="IEnricher{T}" /> type for the element type.
    /// </summary>
    public Type EnricherType { get; }

    /// <summary>
    ///     The <see cref="IEnumerable{T}" /> of <see cref="EnricherType" />, used for service provider lookup.
    /// </summary>
    public Type EnrichersServiceType { get; }

    /// <summary>
    ///     The BulkEnrichAsync method on <see cref="EnricherType" />.
    /// </summary>
    public MethodInfo BulkEnrichMethod { get; }

    /// <summary>
    ///     The EnrichAsync method on <see cref="EnricherType" />.
    /// </summary>
    public MethodInfo EnrichMethod { get; }

    /// <summary>
    ///     Creates a new <see cref="EnricherTypeInfo" /> for the given element type.
    /// </summary>
    /// <param name="elementType">The element type to create enricher type info for.</param>
    public EnricherTypeInfo(Type elementType)
    {
        EnricherType = typeof(IEnricher<>).MakeGenericType(elementType);
        EnrichersServiceType = typeof(IEnumerable<>).MakeGenericType(EnricherType);
        BulkEnrichMethod = EnricherType.GetMethod("BulkEnrichAsync")!;
        EnrichMethod = EnricherType.GetMethod("EnrichAsync")!;
    }
}
