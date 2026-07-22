namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Describes one bound parameter of a CQRS endpoint, after the same normalization
///     that ASP.NET Core minimal API applies (e.g. <c>[AsParameters]</c> expanded, implicit body inferred).
/// </summary>
public sealed class EndpointParameterDescriptor
{
    /// <summary>The parameter name (as it appears in the handler signature or expanded property).</summary>
    public required string Name { get; init; }

    /// <summary>The CLR type of the parameter.</summary>
    public required Type ClrType { get; init; }

    /// <summary>The wire binding source.</summary>
    public required ParameterSource Source { get; init; }

    /// <summary>Whether the parameter is nullable.</summary>
    public bool IsNullable { get; init; }

    /// <summary>Whether the parameter has a default value.</summary>
    public bool HasDefaultValue { get; init; }

    /// <summary>The default value when <see cref="HasDefaultValue"/> is <c>true</c>.</summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    ///     The route key bound for Route parameters: the matching route-template token when one exists, otherwise
    ///     the parameter name that <c>RequestDelegateFactory</c> binds for an explicit <c>[FromRoute]</c>.
    ///     <c>null</c> when <see cref="Source" /> is not <see cref="ParameterSource.Route" />.
    /// </summary>
    public string? RouteToken { get; init; }
}
