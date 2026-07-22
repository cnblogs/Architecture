using System.ComponentModel;
using System.IO.Pipelines;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Builds <see cref="CqrsEndpointDescriptor" /> from a mapped CQRS handler delegate, normalizing its
///     parameters the same way ASP.NET Core minimal API (<c>RequestDelegateFactory</c>) binds them
///     (<c>[FromBody]</c>, <c>[AsParameters]</c> expansion, implicit body inference, route-vs-query by token
///     match, and exclusion of server-injected parameters). Generic overloads (<c>MapQuery&lt;T&gt;</c>,
///     <c>MapPostCommand&lt;T&gt;</c>, ...) synthesize a delegate and reach the same delegate overloads, so this
///     builder handles both the generic and the delegate forms uniformly.
/// </summary>
public static class CqrsEndpointDescriptorBuilder
{
    // Matches a route parameter token such as {id}, {id:int} or {*name}, but not an escaped {{literal}}.
    private static readonly Regex RouteTokenRegex = new(@"(?<!\{)\{\*?([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

    /// <summary>
    ///     Build a descriptor for a mapped CQRS endpoint.
    /// </summary>
    /// <param name="handler">The handler delegate (the synthesized one for generic overloads, or the user's lambda).</param>
    /// <param name="httpMethod">The HTTP verb, e.g. <c>"GET"</c>, <c>"POST"</c>.</param>
    /// <param name="relativeRoute">The route template as passed to the mapper (without the route-group prefix).</param>
    /// <param name="isQuery">Whether this is a query (GET) endpoint.</param>
    /// <param name="requestType">The query/command type.</param>
    /// <param name="responseType">The view/result type, or <c>null</c> for <c>ICommand&lt;TError&gt;</c>.</param>
    /// <param name="errorType">The command error type, or <c>null</c> for queries.</param>
    /// <param name="enableHead">Whether <c>HEAD</c> is also mapped.</param>
    /// <param name="mapNullableRouteParametersEnabled">
    ///     Whether <c>MapNullableRouteParameter.Enable</c> was used. Only then does the mapper register
    ///     substituted routes, so only then are <see cref="CqrsEndpointDescriptor.NullableRouteParameters" /> populated.
    /// </param>
    public static CqrsEndpointDescriptor Build(
        Delegate handler,
        string httpMethod,
        string relativeRoute,
        bool isQuery,
        Type requestType,
        Type? responseType,
        Type? errorType,
        bool enableHead = false,
        bool mapNullableRouteParametersEnabled = false)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var routeTokens = ExtractRouteTokens(relativeRoute);
        var parameters = NormalizeParameters(handler.Method.GetParameters(), routeTokens, nullabilityContext);
        var nullableRouteParameters = mapNullableRouteParametersEnabled
            ? GetNullableRouteParameters(requestType, routeTokens, nullabilityContext)
            : Array.Empty<string>();

        return new CqrsEndpointDescriptor
        {
            HttpMethod = httpMethod,
            RelativeRoute = relativeRoute,
            IsQuery = isQuery,
            RequestType = requestType,
            ResponseType = responseType,
            ErrorType = errorType,
            ResponseShape = DetermineResponseShape(isQuery, responseType),
            PayloadType = parameters.FirstOrDefault(p => p.Source == ParameterSource.Body)?.ClrType,
            Parameters = parameters,
            EnableHead = enableHead,
            NullableRouteParameters = nullableRouteParameters
        };
    }

    /// <summary>
    ///     Normalize handler parameters into <see cref="EndpointParameterDescriptor" />s, mirroring
    ///     <c>RequestDelegateFactory</c> binding rules.
    /// </summary>
    internal static IReadOnlyList<EndpointParameterDescriptor> NormalizeParameters(
        IReadOnlyList<ParameterInfo> parameters,
        IReadOnlySet<string> routeTokens,
        NullabilityInfoContext nullabilityContext)
    {
        var result = new List<EndpointParameterDescriptor>();
        var bodyCandidates = new List<EndpointParameterDescriptor>();

        foreach (var parameter in parameters)
        {
            var clrType = parameter.ParameterType;

            // Server-injected parameters (CancellationToken, HttpContext, [FromServices], ...) and non-body wire
            // bindings ([FromHeader], [FromForm]) are never part of a generated client method signature, so skip them.
            if (IsSpecialType(clrType)
                || HasAttribute(parameter, "FromServicesAttribute")
                || HasAttribute(parameter, "FromKeyedServicesAttribute")
                || HasAttribute(parameter, "FromHeaderAttribute")
                || HasAttribute(parameter, "FromFormAttribute"))
            {
                continue;
            }

            if (HasAttribute(parameter, "FromBodyAttribute"))
            {
                result.Add(FromParameter(parameter, ParameterSource.Body, null, nullabilityContext));
            }
            else if (HasAttribute(parameter, "AsParametersAttribute"))
            {
                ExpandProperties(clrType, routeTokens, nullabilityContext, result);
            }
            else if (HasAttribute(parameter, "FromRouteAttribute"))
            {
                // For an explicit [FromRoute], RequestDelegateFactory uses the parameter name as the route key,
                // even when no matching token appears in the template.
                var token = MatchToken(parameter.Name!, routeTokens) ?? parameter.Name;
                result.Add(FromParameter(parameter, ParameterSource.Route, token, nullabilityContext));
            }
            else if (HasAttribute(parameter, "FromQueryAttribute"))
            {
                result.Add(FromParameter(parameter, ParameterSource.Query, null, nullabilityContext));
            }
            else if (IsSimpleType(clrType))
            {
                var token = MatchToken(parameter.Name!, routeTokens);
                var source = token is null ? ParameterSource.Query : ParameterSource.Route;
                result.Add(FromParameter(parameter, source, token, nullabilityContext));
            }
            else
            {
                // Complex type without an explicit binding attribute: minimal API infers the single one as [FromBody].
                bodyCandidates.Add(FromParameter(parameter, ParameterSource.Body, null, nullabilityContext));
            }
        }

        if (bodyCandidates.Count > 0)
        {
            // minimal API allows at most one body parameter. The first inferred body wins; extras are dropped
            // (the framework itself rejects an endpoint that infers more than one body).
            result.Add(bodyCandidates[0]);
        }

        return result;
    }

    private static void ExpandProperties(
        Type type,
        IReadOnlySet<string> routeTokens,
        NullabilityInfoContext nullabilityContext,
        List<EndpointParameterDescriptor> result)
    {
        // [AsParameters] is only used for GET queries and DELETE commands, neither of which allows a request body,
        // so every property (including complex ones such as PagingParams) is bound from the route or query string.
        foreach (var property in GetBindableProperties(type))
        {
            var token = MatchToken(property.Name, routeTokens);
            var source = token is null ? ParameterSource.Query : ParameterSource.Route;
            result.Add(FromProperty(property, source, token, nullabilityContext));
        }
    }

    private static EndpointParameterDescriptor FromParameter(
        ParameterInfo parameter, ParameterSource source, string? routeToken, NullabilityInfoContext context)
    {
        return new EndpointParameterDescriptor
        {
            Name = parameter.Name!,
            ClrType = parameter.ParameterType,
            Source = source,
            IsNullable = IsNullable(parameter, context),
            HasDefaultValue = parameter.HasDefaultValue,
            DefaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : null,
            RouteToken = routeToken
        };
    }

    private static EndpointParameterDescriptor FromProperty(
        PropertyInfo property, ParameterSource source, string? routeToken, NullabilityInfoContext context)
    {
        // PropertyInfo has no HasDefaultValue; honor [DefaultValue(...)] like RequestDelegateFactory does.
        var defaultValue = property.GetCustomAttribute<DefaultValueAttribute>();
        return new EndpointParameterDescriptor
        {
            Name = property.Name,
            ClrType = property.PropertyType,
            Source = source,
            IsNullable = IsNullableProperty(property, context),
            HasDefaultValue = defaultValue is not null,
            DefaultValue = defaultValue?.Value,
            RouteToken = routeToken
        };
    }

    private static IReadOnlyList<string> GetNullableRouteParameters(
        Type requestType, IReadOnlySet<string> routeTokens, NullabilityInfoContext context)
    {
        // Mirrors CqrsRouteMapper.MapQuery: a route token is expandable when the matching property is nullable
        // (read-state of the getter return parameter) and its name matches a route token.
        return GetBindableProperties(requestType)
            .Where(p => IsNullableProperty(p, context) && MatchToken(p.Name, routeTokens) is not null)
            .Select(p => p.Name)
            .ToList();
    }

    private static ResponseShape DetermineResponseShape(bool isQuery, Type? responseType)
    {
        if (responseType is null)
        {
            return ResponseShape.None;
        }

        if (!isQuery)
        {
            return ResponseShape.Item;
        }

        if (IsPagedList(responseType))
        {
            return ResponseShape.PagedList;
        }

        return IsListType(responseType) ? ResponseShape.List : ResponseShape.Item;
    }

    private static bool IsPagedList(Type type)
    {
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedList<>))
            || typeof(IPagedList).IsAssignableFrom(type);
    }

    private static bool IsListType(Type type)
    {
        if (type.IsArray)
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        return definition == typeof(List<>)
            || definition == typeof(IList<>)
            || definition == typeof(ICollection<>)
            || definition == typeof(IEnumerable<>)
            || definition == typeof(IReadOnlyList<>)
            || definition == typeof(IReadOnlyCollection<>);
    }

    private static IReadOnlySet<string> ExtractRouteTokens(string route)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in RouteTokenRegex.Matches(route))
        {
            tokens.Add(match.Groups[1].Value);
        }

        return tokens;
    }

    private static string? MatchToken(string name, IReadOnlySet<string> tokens)
    {
        foreach (var token in tokens)
        {
            if (string.Equals(token, name, StringComparison.OrdinalIgnoreCase))
            {
                return token;
            }
        }

        return null;
    }

    private static bool IsSimpleType(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (t.IsPrimitive || t.IsEnum)
        {
            return true;
        }

        return t == typeof(string)
            || t == typeof(decimal)
            || t == typeof(Guid)
            || t == typeof(DateTime)
            || t == typeof(DateTimeOffset)
            || t == typeof(TimeSpan)
            || t == typeof(DateOnly)
            || t == typeof(TimeOnly)
            || t == typeof(Uri);
    }

    private static bool IsSpecialType(Type type)
    {
        return type == typeof(CancellationToken)
            || type == typeof(HttpContext)
            || type == typeof(HttpRequest)
            || type == typeof(HttpResponse)
            || typeof(ClaimsPrincipal).IsAssignableFrom(type)
            || type == typeof(IFormFile)
            || typeof(IFormFileCollection).IsAssignableFrom(type)
            || typeof(IFormCollection).IsAssignableFrom(type)
            || type == typeof(Stream)
            || type == typeof(PipeReader)
            || typeof(IServiceProvider).IsAssignableFrom(type);
    }

    private static IEnumerable<PropertyInfo> GetBindableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetMethod != null && p.SetMethod != null);
    }

    private static bool IsNullable(ParameterInfo parameter, NullabilityInfoContext context)
    {
        try
        {
            return context.Create(parameter).ReadState == NullabilityState.Nullable;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    // Mirrors CqrsRouteMapper.MapQuery: uses the read-state of the property getter's return parameter, so the
    // descriptor advertises exactly the nullable route parameters the mapper actually expands.
    private static bool IsNullableProperty(PropertyInfo property, NullabilityInfoContext context)
    {
        if (property.GetMethod is null)
        {
            return false;
        }

        try
        {
            return context.Create(property.GetMethod.ReturnParameter).ReadState == NullabilityState.Nullable;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool HasAttribute(ParameterInfo parameter, string attributeTypeName)
    {
        return parameter.CustomAttributes.Any(a => a.AttributeType.Name == attributeTypeName);
    }
}
