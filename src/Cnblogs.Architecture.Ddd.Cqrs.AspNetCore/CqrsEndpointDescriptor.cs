namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Metadata attached to every CQRS endpoint (query or command) at registration time. It captures
///     everything a strongly-typed service-agent generator needs, in a form that is agnostic to whether
///     the endpoint was registered via a generic (<c>MapQuery&lt;T&gt;</c>) or a delegate
///     (<c>MapQuery(route, handler)</c>) overload.
/// </summary>
public sealed class CqrsEndpointDescriptor
{
    /// <summary>The HTTP verb (e.g. <c>"GET"</c>, <c>"POST"</c>). When <see cref="EnableHead"/>, <c>"HEAD"</c> is also allowed.</summary>
    public required string HttpMethod { get; init; }

    /// <summary>The route template as passed to the mapper (without the route-group prefix); used for route-token analysis.</summary>
    public required string RelativeRoute { get; init; }

    /// <summary>Whether this is a query (GET) endpoint.</summary>
    public bool IsQuery { get; init; }

    /// <summary>The query/command type — used to derive the generated client method name.</summary>
    public required Type RequestType { get; init; }

    /// <summary>The view/result type returned to the client.</summary>
    public Type? ResponseType { get; init; }

    /// <summary>The error type for commands (the <c>TError</c> of <c>ICommand&lt;,&gt;</c>); <c>null</c> for queries.</summary>
    public Type? ErrorType { get; init; }

    /// <summary>The response shape, derived from <see cref="ResponseType"/> and <see cref="IsQuery"/>.</summary>
    public ResponseShape ResponseShape { get; init; }

    /// <summary>The request body type, if any parameter is bound from the body; otherwise <c>null</c>.</summary>
    public Type? PayloadType { get; init; }

    /// <summary>The normalized method parameters (route/query/body), with <c>[AsParameters]</c> expanded.</summary>
    public required IReadOnlyList<EndpointParameterDescriptor> Parameters { get; init; }

    /// <summary>Whether <c>HEAD</c> is also mapped (<c>MapQuery</c> with <c>enableHead</c>).</summary>
    public bool EnableHead { get; init; }

    /// <summary>
    ///     Names of the route tokens whose corresponding request property is nullable. The mapper registers one
    ///     route per subset of these (via <c>MapNullableRouteParameter</c>); the generator collapses them into a
    ///     single client method that substitutes <c>"-"</c> for missing values.
    /// </summary>
    public IReadOnlyList<string> NullableRouteParameters { get; init; } = [];
}
