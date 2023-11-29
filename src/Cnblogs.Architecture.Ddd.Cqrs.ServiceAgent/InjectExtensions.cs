using System.Net;
using System.Net.Http.Headers;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
///     Inject helper for service agent
/// </summary>
public static class InjectExtensions
{
    /// <summary>
    ///     Inject a service agent to services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="baseUri">The base uri for api.</param>
    /// <param name="policy">The polly policy for underlying httpclient.</param>
    /// <typeparam name="TClient">The type of service agent</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddServiceAgent<TClient>(
        this IServiceCollection services,
        string baseUri,
        IAsyncPolicy<HttpResponseMessage>? policy = null)
        where TClient : class
    {
        policy ??= GetDefaultPolicy();
        return services.AddHttpClient<TClient>(
            h =>
            {
                h.BaseAddress = new Uri(baseUri);
                h.AddCqrsAcceptHeaders();
            }).AddPolicyHandler(policy);
    }

    /// <summary>
    ///     Inject a service agent to services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="baseUri">The base uri for api.</param>
    /// <param name="policy">The polly policy for underlying httpclient.</param>
    /// <typeparam name="TClient">The type of api client.</typeparam>
    /// <typeparam name="TImplementation">The type of service agent</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddServiceAgent<TClient, TImplementation>(
        this IServiceCollection services,
        string baseUri,
        IAsyncPolicy<HttpResponseMessage>? policy = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        policy ??= GetDefaultPolicy();
        return services.AddHttpClient<TClient, TImplementation>(
            h =>
            {
                h.BaseAddress = new Uri(baseUri);
                h.AddCqrsAcceptHeaders();
            }).AddPolicyHandler(policy);
    }

    private static void AddCqrsAcceptHeaders(this HttpClient h)
    {
        h.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
        h.DefaultRequestHeaders.AppendCurrentCqrsVersion();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetDefaultPolicy()
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(1500));
    }
}
