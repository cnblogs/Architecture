using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Extension methods for creating cqrs result.
/// </summary>
public static class CqrsResultExtensions
{
    /// <summary>
    ///     Write result as json and append cqrs response header.
    /// </summary>
    /// <param name="extensions"><see cref="IResultExtensions"/></param>
    /// <param name="result">The command response.</param>
    /// <param name="options">Optional json serializer options.</param>
    /// <returns></returns>
    public static IResult Cqrs(this IResultExtensions extensions, object result, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        return new CqrsResult(result, options);
    }
}
