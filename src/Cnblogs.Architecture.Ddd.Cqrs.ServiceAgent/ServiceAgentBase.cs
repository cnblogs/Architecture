using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;

using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
/// ServiceAgent 的基础类。
/// </summary>
/// <typeparam name="TException">异常类型。</typeparam>
public abstract class ServiceAgentBase<TException>
    where TException : Exception, IApiException<TException>
{
    /// <summary>
    /// 构造一个 <see cref="ServiceAgentBase{TException}"/>
    /// </summary>
    /// <param name="httpClient">用于访问 API 的 <see cref="HttpClient"/>。</param>
    protected ServiceAgentBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>
    /// 用于访问 API 的 <see cref="HttpClient"/>。
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    ///     发送一个 DELETE 请求。
    /// </summary>
    /// <param name="url">目标 API 路径。</param>
    /// <typeparam name="TResponse">返回结果类型。</typeparam>
    /// <returns>返回结果。</returns>
    public async Task<TResponse?> DeleteCommandAsync<TResponse>(string url)
    {
        try
        {
            return await HttpClient.DeleteFromJsonAsync<TResponse>(url);
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Delete, url, e);
            return default;
        }
    }

    /// <summary>
    ///     发起一个 DELETE 请求。
    /// </summary>
    /// <param name="url">API 路径。</param>
    public async Task DeleteCommandAsync(string url)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.DeleteAsync(url);
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Delete, url, e);
            return;
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Delete, response.StatusCode, url, content);
        }
    }

    /// <summary>
    ///     发起一个 POST 请求。
    /// </summary>
    /// <param name="url">路径。</param>
    public async Task PostCommandAsync(string url)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsync(url, new StringContent(string.Empty));
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Post, url, e);
            return;
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Post, response.StatusCode, url, content);
        }
    }

    /// <summary>
    ///     发起一个带 Body 的 POST 请求。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <param name="payload">请求。</param>
    /// <typeparam name="TPayload">请求类型。</typeparam>
    public async Task PostCommandAsync<TPayload>(string url, TPayload payload)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsJsonAsync(url, payload);
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Post, url, payload, e);
            return;
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Post, response.StatusCode, url, payload, content);
        }
    }

    /// <summary>
    ///     发起一个带 Body 的 POST 请求。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <param name="payload">请求。</param>
    /// <typeparam name="TResponse">返回类型。</typeparam>
    /// <typeparam name="TPayload">请求类型。</typeparam>
    /// <returns></returns>
    public async Task<TResponse> PostCommandAsync<TResponse, TPayload>(string url, TPayload payload)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsJsonAsync(url, payload);
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Post, url, payload, e);
            return default!; // should never reach here
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Post, response.StatusCode, url, payload, content);
            return default!; // should never reach here
        }

        var responseObj = await response.Content.ReadFromJsonAsync<TResponse>();
        if (responseObj == null)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Post, response.StatusCode, url, payload, content);
            return default!; // should never reach here
        }

        return responseObj;
    }

    /// <summary>
    ///     发起一个 PUT 请求。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <param name="payload">请求内容。</param>
    /// <typeparam name="TResponse">返回结果类型。</typeparam>
    /// <typeparam name="TPayload">请求类型。</typeparam>
    /// <returns></returns>
    public async Task<TResponse> PutCommandAsync<TResponse, TPayload>(string url, TPayload payload)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PutAsJsonAsync(url, payload);
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Put, url, payload, e);
            return default!; // should never reach
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Put, response.StatusCode, url, payload, content);
            return default!;
        }

        var responseObj = await response.Content.ReadFromJsonAsync<TResponse>();
        if (responseObj == null)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Put, response.StatusCode, url, payload, content);
            return default!;
        }

        return responseObj;
    }

    /// <summary>
    ///     获取内容。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <typeparam name="T">结果类型。</typeparam>
    public async Task<T?> GetItemAsync<T>(string url)
    {
        try
        {
            return await HttpClient.GetFromJsonAsync<T>(url);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            ThrowApiException(HttpMethod.Get, url, e);
            return default;
        }
    }

    /// <summary>
    ///     批量获取实体。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <param name="paramName">参数名称。</param>
    /// <param name="ids">主键列表。</param>
    /// <typeparam name="TResponse">返回类型。</typeparam>
    /// <typeparam name="TId">主键类型。</typeparam>
    /// <returns></returns>
    public async Task<List<TResponse>> BatchGetItemsAsync<TResponse, TId>(
        string url,
        string paramName,
        IEnumerable<TId> ids)
        where TId : notnull
    {
        try
        {
            var query = string.Join(
                '&',
                ids.Select(i => $"{WebUtility.UrlEncode(paramName)}={WebUtility.UrlEncode(i.ToString())}"));
            url = $"{url}{(url.Contains('?') ? '&' : '?')}{query}";
            return await HttpClient.GetFromJsonAsync<List<TResponse>>(url) ?? new List<TResponse>();
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode.HasValue)
            {
                ThrowApiException(HttpMethod.Get, e.StatusCode.Value, url, string.Empty);
            }
            else
            {
                ThrowApiException(HttpMethod.Get, url, e);
            }

            return new List<TResponse>();
        }
    }

    /// <summary>
    ///     获取分页列表。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <param name="pagingParams">页码。</param>
    /// <param name="orderByString">分页大小。</param>
    /// <typeparam name="TItem">实体类型。</typeparam>
    /// <returns></returns>
    public async Task<PagedList<TItem>> ListPagedItemsAsync<TItem>(
        string url,
        PagingParams? pagingParams = null,
        string? orderByString = null)
    {
        return await ListPagedItemsAsync<TItem>(url, pagingParams?.PageIndex, pagingParams?.PageSize, orderByString);
    }

    /// <summary>
    ///     获取分页列表。
    /// </summary>
    /// <param name="url">路径。</param>
    /// <param name="pageIndex">页码。</param>
    /// <param name="pageSize">分页大小。</param>
    /// <param name="orderByString">排序字符串。</param>
    /// <typeparam name="TItem">实体类型。</typeparam>
    public async Task<PagedList<TItem>> ListPagedItemsAsync<TItem>(
        string url,
        int? pageIndex,
        int? pageSize,
        string? orderByString = null)
    {
        try
        {
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                var query = $"pageIndex={pageIndex}&pageSize={pageSize}&orderByString={orderByString}";
                url = url.Contains('?') ? url + "&" + query : url + "?" + query;
            }

            return await HttpClient.GetFromJsonAsync<PagedList<TItem>>(url) ?? new PagedList<TItem>();
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode.HasValue)
            {
                ThrowApiException(HttpMethod.Get, e.StatusCode.Value, url, string.Empty);
            }
            else
            {
                ThrowApiException(HttpMethod.Get, url, e);
            }

            return new PagedList<TItem>();
        }
    }

    /// <summary>
    ///     处理抛出异常的情况。
    /// </summary>
    /// <param name="method">请求方法。</param>
    /// <param name="statusCode">状态码，若不适用则是 -1。</param>
    /// <param name="url">请求的 Url</param>
    /// <param name="requestBody">请求内容。</param>
    /// <param name="response">返回内容。</param>
    /// <param name="e">异常。</param>
    [DoesNotReturn]
    protected virtual void ThrowApiException(
        HttpMethod method,
        int statusCode,
        string url,
        object? requestBody,
        string? response,
        Exception? e)
    {
        var message = response ?? e?.Message;
        throw TException.Create(statusCode, $"{method} {url} failed with error: {message}");
    }

    private void ThrowApiException(HttpMethod method, HttpStatusCode statusCode, string url, string responseString)
        => ThrowApiException(method, (int)statusCode, url, null, responseString, null);

    private void ThrowApiException(HttpMethod method, string url, Exception e)
        => ThrowApiException(method, -1, url, null, null, e);

    private void ThrowApiException(
        HttpMethod method,
        HttpStatusCode statusCode,
        string url,
        object? body,
        string responseString)
        => ThrowApiException(method, (int)statusCode, url, body, responseString, null);

    private void ThrowApiException(HttpMethod method, string url, object? body, Exception e)
        => ThrowApiException(method, -1, url, body, null, e);
}