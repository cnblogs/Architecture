using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Loads XML documentation files shipped alongside scanned assemblies and resolves type summaries and constructor parameter
///     descriptions. Results are cached per assembly; an assembly without an <c>.xml</c> sidecar yields <see langword="null" />.
/// </summary>
internal sealed class XmlDocumentationProvider
{
    private readonly ConcurrentDictionary<Assembly, XDocument?> _cache = new();

    /// <summary>
    ///     Returns the <c>&lt;summary&gt;</c> text for <paramref name="type" />, or <see langword="null" /> when unavailable.
    /// </summary>
    public string? GetTypeSummary(Type type)
    {
        return Normalize(GetMemberElement(type.Assembly, "T:" + type.FullName)?.Element("summary")?.Value);
    }

    /// <summary>
    ///     Returns the <c>&lt;param&gt;</c> text for <paramref name="parameter" /> of its declaring constructor, or <see langword="null" />.
    /// </summary>
    public string? GetParameterDescription(ParameterInfo parameter)
    {
        var declaringType = parameter.Member.DeclaringType;
        if (declaringType is null)
        {
            return null;
        }

        var ctorElement = GetMemberElement(declaringType.Assembly, "M:" + declaringType.FullName + ".#ctor");
        return Normalize(ctorElement?.Elements("param")
            .FirstOrDefault(p => p.Attribute("name")?.Value == parameter.Name)?
            .Value);
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
