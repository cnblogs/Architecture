using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>One bound parameter of a generated client method.</summary>
public sealed class ManifestParameter
{
    /// <summary>The parameter name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The wire binding source.</summary>
    public ParameterSource Source { get; set; }

    /// <summary>The parameter CLR type.</summary>
    public ClrTypeRef ClrType { get; set; } = new();

    /// <summary>Whether the parameter is nullable.</summary>
    public bool IsNullable { get; set; }

    /// <summary>Whether the parameter has a default value.</summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>A C#-literal representation of the default value (e.g. <c>true</c>, <c>0</c>, <c>null</c>), or <c>null</c> when unknown.</summary>
    public string? DefaultValueLiteral { get; set; }

    /// <summary>The route token for Route parameters (see <see cref="EndpointParameterDescriptor.RouteToken" />).</summary>
    public string? RouteToken { get; set; }
}
