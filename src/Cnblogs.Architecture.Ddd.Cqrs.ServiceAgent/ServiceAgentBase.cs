using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
///     Base class for service agent.
/// </summary>
/// <typeparam name="TException">The type of exception that this service agent throws.</typeparam>
public abstract class ServiceAgentBase<TException>
    where TException : Exception, IApiException<TException>
{
    /// <summary>
    ///     Create a <see cref="ServiceAgentBase{TException}"/>.
    /// </summary>
    /// <param name="httpClient">The underlying <see cref="HttpClient"/> used to access the API.</param>
    protected ServiceAgentBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>
    ///     The underlying <see cref="HttpClient"/>.
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    ///     Execute a command with DELETE method.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <returns>The response.</returns>
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
    ///     Execute a command with DELETE method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
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
    ///     Execute a command with POST method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
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
    ///     Execute a command with POST method and payload.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
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
    ///     Execute a command with POST method and payload.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TResponse">The type of response body.</typeparam>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    /// <returns>The response body.</returns>
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
    ///     Execute a command with PUT method and payload.
    /// </summary>
    /// <param name="url">The route of API.</param>
    public async Task PutCommandAsync(string url)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PutAsync(url, new StringContent(string.Empty));
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Put, url, e);
            return;
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Put, response.StatusCode, url, content);
        }
    }

    /// <summary>
    ///     Execute a command with PUT method and payload.
    /// </summary>
    /// <param name="url">The route of API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    public async Task PutCommandAsync<TPayload>(string url, TPayload payload)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PutAsJsonAsync(url, payload);
        }
        catch (Exception e)
        {
            ThrowApiException(HttpMethod.Put, url, payload, e);
            return;
        }

        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowApiException(HttpMethod.Put, response.StatusCode, url, payload, content);
        }
    }

    /// <summary>
    ///     Execute a command with PUT method and payload.
    /// </summary>
    /// <param name="url">The route of API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TResponse">The type of response body.</typeparam>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    /// <returns>The response body.</returns>
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
    ///     Query item with GET method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <typeparam name="T">The type of item to get.</typeparam>
    /// <returns>The query result, can be null if item does not exists or status code is 404.</returns>
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
    ///     Batch get items with GET method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="paramName">The name of id field.</param>
    /// <param name="ids">The id list.</param>
    /// <typeparam name="TResponse">The type of the query result item.</typeparam>
    /// <typeparam name="TId">The type of the id.</typeparam>
    /// <returns>A list of items that contains id that in <paramref name="ids"/>, the order or count of the items are not guaranteed.</returns>
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
    ///     Get paged list of items based on url.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="pagingParams">The paging parameters, including page size and page index.</param>
    /// <param name="orderByString">Specifies the order of items to return.</param>
    /// <typeparam name="TItem">The type of items to query.</typeparam>
    /// <returns>The paged list of items. An empty list is returned when there is no result.</returns>
    public async Task<PagedList<TItem>> ListPagedItemsAsync<TItem>(
        string url,
        PagingParams? pagingParams = null,
        string? orderByString = null)
    {
        return await ListPagedItemsAsync<TItem>(url, pagingParams?.PageIndex, pagingParams?.PageSize, orderByString);
    }

    /// <summary>
    ///     Get paged list of items based on url.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="orderByString">Specifies the order of items to return.</param>
    /// <typeparam name="TItem">The type of items to query.</typeparam>
    /// <returns>The paged list of items. An empty list is returned when there is no result.</returns>
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
    ///     Throw exceptions.
    /// </summary>
    /// <param name="method">The method for this request.</param>
    /// <param name="statusCode">HTTP status code, -1 if not available.</param>
    /// <param name="url">The URL to request.</param>
    /// <param name="requestBody">The request body.</param>
    /// <param name="response">The response body.</param>
    /// <param name="e">The exception.</param>
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
        throw TException.Create(statusCode, $"{method} {url} failed with error: {message}", message);
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
