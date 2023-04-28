using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

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
    /// <typeparam name="T">The type of service agent</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddServiceAgent<T>(this IServiceCollection services, string baseUri)
        where T : CqrsServiceAgent
    {
        return services.AddHttpClient<T>(
            h =>
            {
                h.BaseAddress = new Uri(baseUri);
                h.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
            });
    }

    /// <summary>
    ///     Inject a service agent to services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="baseUri">The base uri for api.</param>
    /// <typeparam name="TClient">The type of api client.</typeparam>
    /// <typeparam name="TImplementation">The type of service agent</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddServiceAgent<TClient, TImplementation>(
        this IServiceCollection services,
        string baseUri)
        where TClient : class
        where TImplementation : CqrsServiceAgent, TClient
    {
        return services.AddHttpClient<TClient, TImplementation>(
            h =>
            {
                h.BaseAddress = new Uri(baseUri);
                h.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
            });
    }
}
