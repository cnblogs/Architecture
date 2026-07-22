namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     A serialization-friendly reference to a CLR type, carrying enough information for a code generator to render
///     the C# type name (including generics, arrays and <c>Nullable&lt;T&gt;</c>) without loading the original
///     assembly.
/// </summary>
public sealed record ClrTypeRef
{
    /// <summary>The namespace (empty for global types).</summary>
    public string Namespace { get; init; } = string.Empty;

    /// <summary>The simple (unqualified, non-generic-arity) name, including any declaring-type prefix for nested types.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Whether this is a <c>Nullable&lt;T&gt;</c> (rendered with a trailing <c>?</c>).</summary>
    public bool IsNullable { get; init; }

    /// <summary>Whether this is a single-dimensional or jagged array.</summary>
    public bool IsArray { get; init; }

    /// <summary>The array rank (for <c>[,,]</c>); 1 for the common case.</summary>
    public int ArrayRank { get; init; }

    /// <summary>Generic type arguments, in declaration order.</summary>
    public ClrTypeRef[] GenericArguments { get; init; } = [];

    /// <summary>Build a <see cref="ClrTypeRef" /> from a runtime <see cref="Type" />.</summary>
    /// <param name="type">The runtime type to describe.</param>
    /// <returns>A <see cref="ClrTypeRef" /> capturing the type's namespace, name, generics, array rank and nullability.</returns>
    public static ClrTypeRef FromType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            var inner = FromType(underlying);
            return inner with { IsNullable = true };
        }

        if (type.IsArray)
        {
            return new ClrTypeRef
            {
                IsArray = true,
                ArrayRank = type.GetArrayRank(),
                GenericArguments = [FromType(type.GetElementType()!)]
            };
        }

        return new ClrTypeRef
        {
            Namespace = type.Namespace ?? string.Empty,
            Name = GetSimpleName(type),
            GenericArguments = type.IsGenericType
                ? type.GetGenericArguments().Select(FromType).ToArray()
                : []
        };
    }

    private static string GetSimpleName(Type type)
    {
        var name = type.IsGenericType ? type.Name[..type.Name.IndexOf('`')] : type.Name;
        if (type is { IsNested: true, DeclaringType: not null })
        {
            return GetSimpleName(type.DeclaringType) + "." + name;
        }

        return name;
    }
}
