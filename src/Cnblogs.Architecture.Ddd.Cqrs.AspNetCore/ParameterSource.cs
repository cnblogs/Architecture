namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Where a parameter of a CQRS endpoint is bound from on the wire.
/// </summary>
public enum ParameterSource
{
    /// <summary>Bound from a route segment.</summary>
    Route,

    /// <summary>Bound from the query string.</summary>
    Query,

    /// <summary>Bound from the request body.</summary>
    Body
}
