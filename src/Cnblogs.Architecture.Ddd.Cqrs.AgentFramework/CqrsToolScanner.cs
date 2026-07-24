using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Scans assemblies for Command/Query types opted in to agent-tool exposure and builds a <see cref="CqrsToolDescriptor" /> per type.
/// </summary>
internal static class CqrsToolScanner
{
    /// <summary>
    ///     Returns one descriptor per exposed Command/Query in <paramref name="assemblies" />, honoring
    ///     <see cref="CqrsAgentOptions.Discovery" /> and <see cref="AgentToolAttribute.Exposure" />. Throws on duplicate tool names.
    /// </summary>
    public static IReadOnlyList<CqrsToolDescriptor> Scan(
        IReadOnlyList<Assembly> assemblies,
        CqrsAgentOptions options,
        XmlDocumentationProvider xmlDocumentation)
    {
        return ScanTypes(assemblies.SelectMany(a => a.GetTypes()), options, xmlDocumentation);
    }

    /// <summary>
    ///     Same as <see cref="Scan" /> but over an explicit type list, for testability.
    /// </summary>
    public static IReadOnlyList<CqrsToolDescriptor> ScanTypes(
        IEnumerable<Type> types,
        CqrsAgentOptions options,
        XmlDocumentationProvider xmlDocumentation)
    {
        var descriptors = new List<CqrsToolDescriptor>();
        var usedNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var type in types.Where(t => t is { IsClass: true, IsInterface: false, IsAbstract: false, IsGenericTypeDefinition: false }))
        {
            var kind = CqrsRequestInspector.GetKind(type);
            if (kind == RequestKind.None)
            {
                continue;
            }

            var attribute = type.GetCustomAttribute<AgentToolAttribute>(inherit: true);
            var isMarked = attribute is not null || typeof(IAgentTool).IsAssignableFrom(type);
            var exposed = options.Discovery switch
            {
                ToolDiscovery.Marked => isMarked && attribute?.Exposure != ToolExposure.Exclude,
                ToolDiscovery.AllValidRequests => attribute?.Exposure != ToolExposure.Exclude,
                _ => false,
            };
            if (exposed == false)
            {
                continue;
            }

            var descriptor = new CqrsToolDescriptor(type, kind, options, xmlDocumentation);
            if (usedNames.Add(descriptor.Name) == false)
            {
                throw new InvalidOperationException(
                    $"Duplicate agent tool name '{descriptor.Name}' (from {type.FullName}). Use [AgentTool(Name = \"...\")] to disambiguate.");
            }

            descriptors.Add(descriptor);
        }

        return descriptors;
    }
}
