namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Declares that the enricher must run after the specified enricher(s).
///     Only references enrichers within the same <see cref="IEnricher{T}" /> group.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class EnrichAfterAttribute : Attribute
{
    /// <summary>
    ///     The enricher types that must complete before this enricher runs.
    /// </summary>
    public Type[] DependencyTypes { get; }

    /// <summary>
    ///     Creates a new <see cref="EnrichAfterAttribute" /> with the specified dependency enricher types.
    /// </summary>
    /// <param name="dependencyTypes">The enricher types that must complete before this enricher runs.</param>
    public EnrichAfterAttribute(params Type[] dependencyTypes)
    {
        ArgumentNullException.ThrowIfNull(dependencyTypes);
        DependencyTypes = dependencyTypes;
    }
}
