using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     Loads XML documentation files shipped alongside assemblies and resolves type summaries and constructor parameter
///     descriptions. Results are cached per assembly; an assembly without an <c>.xml</c> sidecar yields <see langword="null" />.
/// </summary>
public sealed class XmlDocumentationProvider
{
    private readonly ConcurrentDictionary<Assembly, XDocument?> _cache = new();

    /// <summary>
    ///     Returns the <c>&lt;summary&gt;</c> text for <paramref name="type" />, or <see langword="null" /> when unavailable.
    /// </summary>
    /// <param name="type">The type to look up.</param>
    /// <returns>The summary text, or <see langword="null" /> when the assembly has no XML documentation or the type is undocumented.</returns>
    public string? GetTypeSummary(Type type)
    {
        return Normalize(GetMemberElement(type.Assembly, "T:" + GetDocName(type))?.Element("summary")?.Value);
    }

    /// <summary>
    ///     Returns the <c>&lt;param&gt;</c> text for <paramref name="parameter" /> of its declaring constructor, or <see langword="null" />.
    /// </summary>
    /// <param name="parameter">The constructor parameter to look up.</param>
    /// <returns>The parameter description, or <see langword="null" /> when unavailable.</returns>
    public string? GetParameterDescription(ParameterInfo parameter)
    {
        var declaringType = parameter.Member.DeclaringType;
        if (declaringType is null)
        {
            return null;
        }

        // Parameterized constructors carry a parameter list in their doc id (e.g. "...#ctor(System.Int32)"),
        // so match by prefix; nested types use '.' instead of '+' in doc ids.
        var ctorPrefix = "M:" + GetDocName(declaringType) + ".#ctor";
        var ctorElement = GetDocument(declaringType.Assembly)?.Descendants("member")
            .FirstOrDefault(e => e.Attribute("name")?.Value.StartsWith(ctorPrefix, StringComparison.Ordinal) is true);
        return Normalize(ctorElement?.Elements("param")
            .FirstOrDefault(p => p.Attribute("name")?.Value == parameter.Name)?
            .Value);
    }

    private static string GetDocName(Type type)
    {
        return type.FullName!.Replace('+', '.');
    }

    private static string? Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private XElement? GetMemberElement(Assembly assembly, string memberId)
    {
        return GetDocument(assembly)?.Descendants("member")
            .FirstOrDefault(e => e.Attribute("name")?.Value == memberId);
    }

    private XDocument? GetDocument(Assembly assembly)
    {
        return _cache.GetOrAdd(assembly, static a =>
        {
            var path = Path.ChangeExtension(a.Location, ".xml");
            return File.Exists(path) ? XDocument.Load(path) : null;
        });
    }
}
