using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
///     分页参数。
/// </summary>
/// <param name="PageIndex">页码。</param>
/// <param name="PageSize">每页元素数。</param>
public record PagingParams([Range(1, int.MaxValue)] int PageIndex, [Range(0, int.MaxValue)] int PageSize)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"{PageIndex}-{PageSize}";
    }

    /// <summary>
    ///     供 minimum API 使用
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static ValueTask<PagingParams?> BindAsync(HttpContext context)
    {
        const string pageIndexKey = "pageIndex";
        const string pageSizeKey = "pageSize";

        var routeValues = context.GetRouteData().Values;
        object? pageSizeValue = null;
        var inRouteValues =
            routeValues.TryGetValue(pageIndexKey, out var pageIndexValue) &&
            routeValues.TryGetValue(pageSizeKey, out pageSizeValue);

        if (inRouteValues == false)
        {
            pageIndexValue = context.Request.Query[pageIndexKey];
            pageSizeValue = context.Request.Query[pageSizeKey];
        }

        if (pageIndexValue != null &&
            pageSizeValue != null &&
            int.TryParse(pageIndexValue.ToString(), out var pageIndex) &&
            int.TryParse(pageSizeValue.ToString(), out var pageSize))
        {
            return ValueTask.FromResult<PagingParams?>(new PagingParams(pageIndex, pageSize));
        }

        return ValueTask.FromResult<PagingParams?>(null);
    }
}
