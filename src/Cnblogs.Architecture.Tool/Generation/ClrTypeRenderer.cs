using Cnblogs.Architecture.Tool.Manifest;

namespace Cnblogs.Architecture.Tool.Generation;

/// <summary>
///     Renders a <see cref="ClrTypeRef" /> into the core (non-nullability-annotated) C# type expression, mapping
///     <c>System.*</c> primitives to their C# keywords and collecting the namespaces a source file must import.
///     Nullability (<c>?</c>) is left to the caller, which combines <see cref="ClrTypeRef.IsNullable" />
///     (<c>Nullable&lt;T&gt;</c>) with the per-parameter <c>IsNullable</c> flag (reference nullability).
/// </summary>
internal sealed class ClrTypeRenderer
{
    private static readonly Dictionary<string, string> SystemKeywords = new()
    {
        ["System.SByte"] = "sbyte",
        ["System.Byte"] = "byte",
        ["System.Int16"] = "short",
        ["System.UInt16"] = "ushort",
        ["System.Int32"] = "int",
        ["System.UInt32"] = "uint",
        ["System.Int64"] = "long",
        ["System.UInt64"] = "ulong",
        ["System.IntPtr"] = "nint",
        ["System.UIntPtr"] = "nuint",
        ["System.Char"] = "char",
        ["System.Boolean"] = "bool",
        ["System.Single"] = "float",
        ["System.Double"] = "double",
        ["System.Decimal"] = "decimal",
        ["System.String"] = "string",
        ["System.Object"] = "object",
        ["System.Void"] = "void"
    };

    private readonly HashSet<string> _importedNamespaces = [];

    /// <summary>The namespaces that the rendered types depend on (for emitting <c>using</c> directives).</summary>
    public IReadOnlyCollection<string> ImportedNamespaces => _importedNamespaces;

    /// <summary>
    ///     Render the C# type expression for <paramref name="type" />, recording required namespaces. This includes
    ///     <c>Nullable&lt;T&gt;</c> annotations (rendered as <c>T?</c>) at every nesting level, because
    ///     <see cref="ClrTypeRef.IsNullable" /> is set by the exporter for <c>Nullable&lt;T&gt;</c> value types.
    ///     Reference-type nullability is not carried in <see cref="ClrTypeRef" />; callers combine the per-parameter
    ///     <c>IsNullable</c> flag (set <c>!</c> only when <c>ClrType.IsNullable</c> is already true) to avoid a
    ///     double <c>?</c>.
    /// </summary>
    public string Render(ClrTypeRef? type)
    {
        if (type is null)
        {
            return "object";
        }

        return RenderWithNullability(type);
    }

    private string RenderWithNullability(ClrTypeRef? type)
    {
        if (type is null)
        {
            return "object";
        }

        var core = RenderCore(type);
        return type.IsNullable ? core + "?" : core;
    }

    private string RenderCore(ClrTypeRef type)
    {
        if (type.IsArray)
        {
            var element = type.GenericArguments.Length > 0 ? type.GenericArguments[0] : null;
            var innerCommas = type.ArrayRank > 1 ? new string(',', type.ArrayRank - 1) : string.Empty;
            return RenderWithNullability(element) + "[" + innerCommas + "]";
        }

        var fullName = type.Namespace.Length > 0 ? type.Namespace + "." + type.Name : type.Name;
        if (SystemKeywords.TryGetValue(fullName, out var keyword))
        {
            return keyword;
        }

        if (type.Namespace.Length > 0)
        {
            _importedNamespaces.Add(type.Namespace);
        }

        var name = type.Name;
        if (type.GenericArguments.Length > 0)
        {
            name += "<" + string.Join(", ", type.GenericArguments.Select(RenderWithNullability)) + ">";
        }

        return name;
    }

    /// <summary>Forget previously collected namespaces.</summary>
    public void Reset()
    {
        _importedNamespaces.Clear();
    }
}
