using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

internal static class CqrsVersionExtensions
{
    private const int CurrentCqrsVersion = 2;

    public static int CqrsVersion(this IHeaderDictionary headers)
    {
        return int.TryParse(headers[CqrsHeaderNames.CqrsVersion].ToString(), out var version) ? version : 1;
    }

    public static int CqrsVersion(this HttpHeaders headers)
    {
        if (headers.Contains(CqrsHeaderNames.CqrsVersion) == false)
        {
            return 1;
        }

        return headers.GetValues(CqrsHeaderNames.CqrsVersion).Select(x => int.TryParse(x, out var y) ? y : 1).Max();
    }

    public static void CqrsVersion(this IHeaderDictionary headers, int version)
    {
        headers.Append(CqrsHeaderNames.CqrsVersion, version.ToString());
    }

    public static void AppendCurrentCqrsVersion(this IHeaderDictionary headers)
    {
        headers.CqrsVersion(CurrentCqrsVersion);
    }

    public static void AppendCurrentCqrsVersion(this HttpHeaders headers)
    {
        headers.Add(CqrsHeaderNames.CqrsVersion, CurrentCqrsVersion.ToString());
    }
}
