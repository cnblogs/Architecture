using Microsoft.AspNetCore.Builder;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Endpoint metadata that assigns all endpoints mapped within a route group to one service-agent group,
///     so the generated client emits them under a single <c>IXxxService</c> / <c>XxxService</c> pair.
/// </summary>
public sealed class ServiceAgentGroupMetadata
{
    /// <summary>The group name. Becomes the suffix of the generated <c>IXxxService</c>.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Optional explicit error type (<c>TError</c>) for the group. When omitted, the generator infers it
    ///     from the commands in the group.
    /// </summary>
    public Type? ErrorType { get; init; }
}

/// <summary>
///     Extension methods for tagging route groups with a service-agent group.
/// </summary>
public static class ServiceAgentGroupExtensions
{
    /// <summary>
    ///     Tag every endpoint mapped within <paramref name="builder"/> with the service-agent group
    ///     <paramref name="name"/>. Call this on a route group (the result of <c>MapGroup</c>) before mapping
    ///     queries/commands so the metadata propagates to all child endpoints.
    /// </summary>
    /// <param name="builder">The route group or endpoint convention builder to tag.</param>
    /// <param name="name">The group name. Becomes the suffix of the generated <c>IXxxService</c>.</param>
    /// <param name="errorType">
    ///     Optional explicit error type (<c>TError</c>) for the group. When omitted, the generator infers it
    ///     from the commands in the group.
    /// </param>
    /// <typeparam name="TBuilder">The endpoint convention builder type (e.g. the result of <c>MapGroup</c>).</typeparam>
    public static TBuilder WithServiceAgentGroup<TBuilder>(
        this TBuilder builder,
        string name,
        Type? errorType = null)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new ServiceAgentGroupMetadata { Name = name, ErrorType = errorType });
        return builder;
    }
}
