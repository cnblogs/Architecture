using System.Globalization;
using System.Text.RegularExpressions;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Microsoft.AspNetCore.Routing;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     Builds an <see cref="EndpointManifest" /> from the endpoints registered in an
///     <see cref="EndpointDataSource" />: each CQRS endpoint (carrying a <see cref="CqrsEndpointDescriptor" />) is
///     turned into a <see cref="ManifestEndpoint" />, and endpoints are grouped into <see cref="ManifestGroup" />s
///     that each become one generated service agent.
/// </summary>
public static class EndpointManifestBuilder
{
    private static readonly Regex VersionSegmentRegex = new(@"^v(\d+|\{.+\})$", RegexOptions.Compiled);

    /// <summary>Build the manifest for the given endpoint data source.</summary>
    /// <param name="endpointDataSource">The endpoint data source whose CQRS endpoints (those carrying a <see cref="CqrsEndpointDescriptor" />) are exported.</param>
    /// <returns>An <see cref="EndpointManifest" /> with one group per generated service agent.</returns>
    public static EndpointManifest Build(EndpointDataSource endpointDataSource)
    {
        var records = new List<EndpointRecord>();
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
            {
                continue;
            }

            var descriptor = endpoint.Metadata.GetMetadata<CqrsEndpointDescriptor>();
            if (descriptor is null)
            {
                continue;
            }

            var groupMetadata = endpoint.Metadata.GetMetadata<ServiceAgentGroupMetadata>();
            var rawRoute = routeEndpoint.RoutePattern.RawText ?? string.Empty;
            records.Add(
                new EndpointRecord(
                    BuildEndpoint(routeEndpoint, descriptor, rawRoute),
                    descriptor.ErrorType,
                    groupMetadata?.Name,
                    groupMetadata?.ErrorType,
                    GetFirstSegment(rawRoute)));
        }

        return new EndpointManifest { Groups = BuildGroups(records) };
    }

    private static ManifestEndpoint BuildEndpoint(RouteEndpoint endpoint, CqrsEndpointDescriptor descriptor, string route)
    {
        var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.ToList()
            ?? [descriptor.HttpMethod];
        var primary = methods.Count > 0 ? methods[0] : descriptor.HttpMethod;

        return new ManifestEndpoint
        {
            HttpMethod = primary,
            HttpMethods = methods,
            Route = route,
            IsQuery = descriptor.IsQuery,
            ResponseShape = descriptor.ResponseShape,
            ResponseType = descriptor.ResponseType is null ? null : ClrTypeRef.FromType(descriptor.ResponseType),
            PayloadType = descriptor.PayloadType is null ? null : ClrTypeRef.FromType(descriptor.PayloadType),
            PayloadContract = descriptor.PayloadType == descriptor.RequestType
                ? new ManifestPayloadContract
                {
                    Properties = descriptor.PayloadProperties
                        .Select(p => new ManifestPayloadProperty
                        {
                            Name = p.Name,
                            ClrType = ClrTypeRef.FromType(p.ClrType),
                            IsNullable = p.IsNullable
                        })
                        .ToList()
                }
                : null,
            RequestTypeName = descriptor.RequestType.Name,
            Parameters = descriptor.Parameters.Select(BuildParameter).ToList(),
            NullableRouteParameters = descriptor.NullableRouteParameters.ToList(),
            EnableHead = descriptor.EnableHead
        };
    }

    private static ManifestParameter BuildParameter(EndpointParameterDescriptor parameter)
    {
        return new ManifestParameter
        {
            Name = parameter.Name,
            Source = parameter.Source,
            ClrType = ClrTypeRef.FromType(parameter.ClrType),
            IsNullable = parameter.IsNullable,
            HasDefaultValue = parameter.HasDefaultValue,
            DefaultValueLiteral = parameter.HasDefaultValue ? RenderDefaultLiteral(parameter.DefaultValue) : null,
            RouteToken = parameter.RouteToken
        };
    }

    private static List<ManifestGroup> BuildGroups(List<EndpointRecord> records)
    {
        // Phase 1 — for each first route segment, remember the error type of a command under it, so queries that
        // share the segment can join that command's group.
        var segmentToErrorType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var record in records)
        {
            if (record.ExplicitGroupName is null && record.ErrorType is not null)
            {
                segmentToErrorType.TryAdd(record.FirstSegment, record.ErrorType);
            }
        }

        // Phase 2 — assign a stable group key to each endpoint and collect into buckets.
        var buckets = new Dictionary<string, GroupBucket>(StringComparer.Ordinal);
        foreach (var record in records)
        {
            var (key, name) = ResolveGroupKey(record, segmentToErrorType);
            if (!buckets.TryGetValue(key, out var bucket))
            {
                bucket = new GroupBucket(key, name);
                buckets[key] = bucket;
            }

            bucket.Records.Add(record);
        }

        // Phase 3 — resolve each group's error type and validate consistency.
        var groups = buckets.Values
            .Select(bucket => new ManifestGroup
            {
                Name = bucket.Name,
                ErrorType = ResolveErrorType(bucket),
                Endpoints = bucket.Records.Select(r => r.Endpoint).ToList()
            })
            .OrderBy(g => g.Name, StringComparer.Ordinal)
            .ToList();

        EnsureUniqueGroupNames(groups);
        return groups;
    }

    private static void EnsureUniqueGroupNames(List<ManifestGroup> groups)
    {
        var duplicates = groups.GroupBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException(
                "Service-agent groups resolved to duplicate names ("
                + string.Join(", ", duplicates)
                + "). This usually means an explicit WithServiceAgentGroup name collides with an error-type- or "
                + "segment-derived name; use distinct group names or unify the error types.");
        }
    }

    private static (string Key, string Name) ResolveGroupKey(
        EndpointRecord record,
        IReadOnlyDictionary<string, Type> segmentToErrorType)
    {
        if (record.ExplicitGroupName is not null)
        {
            return ("explicit::" + record.ExplicitGroupName, record.ExplicitGroupName);
        }

        if (record.ErrorType is not null)
        {
            return (ErrorKey(record.ErrorType), StripErrorSuffix(record.ErrorType.Name));
        }

        // Query without an explicit group: join a command group sharing the first route segment if one exists.
        if (record.FirstSegment.Length > 0
            && segmentToErrorType.TryGetValue(record.FirstSegment, out var commandErrorType))
        {
            return (ErrorKey(commandErrorType), StripErrorSuffix(commandErrorType.Name));
        }

        var segmentName = Capitalize(record.FirstSegment);
        return ("segment::" + record.FirstSegment.ToLowerInvariant(), segmentName.Length > 0 ? segmentName : "Default");
    }

    private static ClrTypeRef? ResolveErrorType(GroupBucket bucket)
    {
        // An explicit error type on the group metadata wins; otherwise take the (unique) error type of the
        // group's commands. Mixed error types within one group are invalid — the base class is parameterized by one.
        var explicitTypes = bucket.Records
            .Where(r => r.ExplicitErrorType is not null)
            .Select(r => r.ExplicitErrorType!)
            .Distinct()
            .ToList();
        if (explicitTypes.Count > 1)
        {
            throw new InvalidOperationException(
                $"Service-agent group '{bucket.Name}' has conflicting explicit error types: "
                + string.Join(", ", explicitTypes.Select(t => t.FullName)));
        }

        var commandErrorTypes = bucket.Records
            .Where(r => r.ErrorType is not null)
            .Select(r => r.ErrorType!)
            .Distinct()
            .ToList();
        if (commandErrorTypes.Count > 1)
        {
            throw new InvalidOperationException(
                $"Service-agent group '{bucket.Name}' contains commands with different error types: "
                + string.Join(", ", commandErrorTypes.Select(t => t.FullName))
                + ". Split the route groups with WithServiceAgentGroup, or unify the error types.");
        }

        var type = explicitTypes.FirstOrDefault() ?? commandErrorTypes.FirstOrDefault();
        return type is null ? null : ClrTypeRef.FromType(type);
    }

    private static string ErrorKey(Type errorType)
    {
        var ns = errorType.Namespace ?? string.Empty;
        return "error::" + (ns.Length > 0 ? ns + "." : string.Empty) + errorType.Name;
    }

    private static string StripErrorSuffix(string name)
    {
        return name.EndsWith("Error", StringComparison.Ordinal) && name.Length > "Error".Length
            ? name.Substring(0, name.Length - "Error".Length)
            : name;
    }

    private static string Capitalize(string segment)
    {
        if (segment.Length == 0)
        {
            return segment;
        }

        var sanitized = new string(segment.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        return char.ToUpperInvariant(sanitized[0]) + sanitized.Substring(1);
    }

    private static string GetFirstSegment(string route)
    {
        foreach (var part in route.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.Equals("api", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (VersionSegmentRegex.IsMatch(part))
            {
                continue;
            }

            return part;
        }

        return string.Empty;
    }

    private static string? RenderDefaultLiteral(object? value)
    {
        return value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            char c => "'" + EscapeCharLiteral(c) + "'",
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            decimal d => d.ToString(CultureInfo.InvariantCulture) + "m",
            float f => f.ToString("R", CultureInfo.InvariantCulture) + "f",
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            // DateTime/Guid/... have no concise C# literal; drop the default rather than emit a non-compilable token.
            DateTime or DateTimeOffset or Guid or TimeSpan => null,
            _ when value.GetType().IsEnum => value.GetType().Name + "." + value,
            _ => value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : null
        };
    }

    private static string EscapeCharLiteral(char c)
    {
        return c switch
        {
            '\'' => "\\'",
            '\\' => "\\\\",
            '\n' => "\\n",
            '\r' => "\\r",
            '\t' => "\\t",
            _ => c.ToString()
        };
    }

    private sealed record EndpointRecord(
        ManifestEndpoint Endpoint,
        Type? ErrorType,
        string? ExplicitGroupName,
        Type? ExplicitErrorType,
        string FirstSegment);

    private sealed class GroupBucket(string key, string name)
    {
        public string Key { get; } = key;

        public string Name { get; } = name;

        public List<EndpointRecord> Records { get; } = [];
    }
}
