namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Defines behavior for nullable route parameters.
/// </summary>
public enum MapNullableRouteParameter
{
    /// <summary>
    ///     Map different routes to present <c>null</c> for nullable route parameters.
    /// </summary>
    Enable = 1,

    /// <summary>
    ///     Do not map extra route for nullable route parameters.
    /// </summary>
    Disable = 2
}
