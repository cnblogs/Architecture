using System.Net;
using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
///     Service Agent for CQRS
/// </summary>
public abstract class CqrsServiceAgent
{
    /// <summary>
    ///     The underlying <see cref="HttpClient"/>.
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    ///     Create a service agent for cqrs api.
    /// </summary>
    /// <param name="httpClient">The underlying HttpClient.</param>
    protected CqrsServiceAgent(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>
    ///     Execute a command with DELETE method.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <returns>The response.</returns>
    public async Task<CommandResponse<TResponse, ServiceAgentError>> DeleteCommandAsync<TResponse>(string url)
    {
        var response = await HttpClient.DeleteAsync(url);
        return await HandleCommandResponseAsync<TResponse>(response);
    }

    /// <summary>
    ///     Execute a command with DELETE method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    public async Task<CommandResponse<ServiceAgentError>> DeleteCommandAsync(string url)
    {
        var response = await HttpClient.DeleteAsync(url);
        return await HandleCommandResponseAsync(response);
    }

    /// <summary>
    ///     Execute a command with POST method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    public async Task<CommandResponse<ServiceAgentError>> PostCommandAsync(string url)
    {
        var response = await HttpClient.PostAsync(url, new StringContent(string.Empty));
        return await HandleCommandResponseAsync(response);
    }

    /// <summary>
    ///     Execute a command with POST method and payload.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    public async Task<CommandResponse<ServiceAgentError>> PostCommandAsync<TPayload>(string url, TPayload payload)
    {
        var response = await HttpClient.PostAsJsonAsync(url, payload);
        return await HandleCommandResponseAsync(response);
    }

    /// <summary>
    ///     Execute a command with POST method and payload.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TResponse">The type of response body.</typeparam>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    /// <returns>The response body.</returns>
    public async Task<CommandResponse<TResponse, ServiceAgentError>> PostCommandAsync<TResponse, TPayload>(
        string url,
        TPayload payload)
    {
        var response = await HttpClient.PostAsJsonAsync(url, payload);
        return await HandleCommandResponseAsync<TResponse>(response);
    }

    /// <summary>
    ///     Execute a command with PUT method and payload.
    /// </summary>
    /// <param name="url">The route of API.</param>
    public async Task<CommandResponse<ServiceAgentError>> PutCommandAsync(string url)
    {
        var response = await HttpClient.PutAsync(url, new StringContent(string.Empty));
        return await HandleCommandResponseAsync(response);
    }

    /// <summary>
    ///     Execute a command with PUT method and payload.
    /// </summary>
    /// <param name="url">The route of API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    /// <returns>The command response.</returns>
    public async Task<CommandResponse<ServiceAgentError>> PutCommandAsync<TPayload>(string url, TPayload payload)
    {
        var response = await HttpClient.PutAsJsonAsync(url, payload);
        return await HandleCommandResponseAsync(response);
    }

    /// <summary>
    ///     Execute a command with PUT method and payload.
    /// </summary>
    /// <param name="url">The route of API.</param>
    /// <param name="payload">The request body.</param>
    /// <typeparam name="TResponse">The type of response body.</typeparam>
    /// <typeparam name="TPayload">The type of request body.</typeparam>
    /// <returns>The response body.</returns>
    public async Task<CommandResponse<TResponse, ServiceAgentError>> PutCommandAsync<TResponse, TPayload>(
        string url,
        TPayload payload)
    {
        var response = await HttpClient.PutAsJsonAsync(url, payload);
        return await HandleCommandResponseAsync<TResponse>(response);
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

            throw;
        }
    }

    /// <summary>
    ///     Query item with HEAD method.
    /// </summary>
    /// <param name="url">The route of the API.</param>
    /// <returns>True if status code is 2xx, False if status code is 404.</returns>
    public async Task<bool> HasItemAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await HttpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode(); // throw for other status code
        return false;
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
        var query = string.Join(
            '&',
            ids.Select(i => $"{WebUtility.UrlEncode(paramName)}={WebUtility.UrlEncode(i.ToString())}"));
        url = $"{url}{(url.Contains('?') ? '&' : '?')}{query}";
        return await HttpClient.GetFromJsonAsync<List<TResponse>>(url) ?? new List<TResponse>();
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
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            var query = $"pageIndex={pageIndex}&pageSize={pageSize}&orderByString={orderByString}";
            url = url.Contains('?') ? url + "&" + query : url + "?" + query;
        }

        return await HttpClient.GetFromJsonAsync<PagedList<TItem>>(url) ?? new PagedList<TItem>();
    }

    private static async Task<CommandResponse<TResponse, ServiceAgentError>> HandleCommandResponseAsync<TResponse>(
        HttpResponseMessage httpResponseMessage)
    {
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();
            return CommandResponse<TResponse, ServiceAgentError>.Success(result);
        }

        var response = await httpResponseMessage.Content.ReadFromJsonAsync<CommandResponse>();
        if (response is null)
        {
            return CommandResponse<TResponse, ServiceAgentError>.Fail(ServiceAgentError.UnknownError);
        }

        return new CommandResponse<TResponse, ServiceAgentError>
        {
            IsConcurrentError = response.IsConcurrentError,
            IsValidationError = response.IsValidationError,
            ErrorMessage = response.ErrorMessage,
            LockAcquired = response.LockAcquired,
            ValidationErrors = response.ValidationErrors,
            ErrorCode = new ServiceAgentError(1, response.ErrorMessage)
        };
    }

    private static async Task<CommandResponse<ServiceAgentError>> HandleCommandResponseAsync(
        HttpResponseMessage message)
    {
        if (message.IsSuccessStatusCode)
        {
            return CommandResponse<ServiceAgentError>.Success();
        }

        var response = await message.Content.ReadFromJsonAsync<CommandResponse>();
        if (response is null)
        {
            return CommandResponse<ServiceAgentError>.Fail(ServiceAgentError.UnknownError);
        }

        return new CommandResponse<ServiceAgentError>
        {
            IsConcurrentError = response.IsConcurrentError,
            IsValidationError = response.IsValidationError,
            ErrorMessage = response.ErrorMessage,
            LockAcquired = response.LockAcquired,
            ValidationErrors = response.ValidationErrors,
            ErrorCode = new ServiceAgentError(1, response.ErrorMessage)
        };
    }
}
