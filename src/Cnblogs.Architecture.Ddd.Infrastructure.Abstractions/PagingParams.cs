using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

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
        var hasPageIndex = int.TryParse(context.Request.Query[pageIndexKey], out var pageIndex);
        var hasPageSize = int.TryParse(context.Request.Query[pageSizeKey], out var pageSize);
        if (hasPageIndex && hasPageSize && pageIndex > 0 && pageSize >= 0)
        {
            return ValueTask.FromResult<PagingParams?>(new PagingParams(pageIndex, pageSize));
        }

        return ValueTask.FromResult<PagingParams?>(null);
    }
}