using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>One CQRS endpoint as seen by the generator.</summary>
public sealed class ManifestEndpoint
{
    /// <summary>The primary HTTP verb (<c>"GET"</c>, <c>"POST"</c>, ...).</summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>All HTTP verbs mapped (e.g. <c>["GET", "HEAD"]</c>).</summary>
    public List<string> HttpMethods { get; set; } = [];

    /// <summary>The full route template (including the route-group prefix and constraints), e.g. <c>/api/v1/products/{id:int}</c>.</summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>Whether this is a query (GET) endpoint.</summary>
    public bool IsQuery { get; set; }

    /// <summary>The response shape, derived from the query/command type.</summary>
    public ResponseShape ResponseShape { get; set; }

    /// <summary>The view/result type, or <c>null</c> for fire-and-forget commands.</summary>
    public ClrTypeRef? ResponseType { get; set; }

    /// <summary>The request body type, or <c>null</c> when there is no body.</summary>
    public ClrTypeRef? PayloadType { get; set; }

    /// <summary>
    ///     The payload POCO contract to generate when the body type is the command itself (so the client need not
    ///     reference the command's assembly); <c>null</c> for delegate-form bodies (a separate DTO) or bodyless endpoints.
    /// </summary>
    public ManifestPayloadContract? PayloadContract { get; set; }

    /// <summary>The query/command type name, used to derive the generated method name.</summary>
    public string RequestTypeName { get; set; } = string.Empty;

    /// <summary>The normalized method parameters (route/query/body).</summary>
    public List<ManifestParameter> Parameters { get; set; } = [];

    /// <summary>Names of nullable route tokens expanded into multiple routes (the generator collapses these).</summary>
    public List<string> NullableRouteParameters { get; set; } = [];

    /// <summary>Whether <c>HEAD</c> is also mapped (the generator emits an extra <c>HasXxxAsync</c>).</summary>
    public bool EnableHead { get; set; }
}
