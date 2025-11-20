using System.Net.Http.Headers;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Http.Resilience;

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
    /// <param name="loggingConfigure">Configure logging behavior.</param>
    /// <param name="pollyConfigure">The polly policy for underlying httpclient.</param>
    /// <typeparam name="TClient">The type of service agent</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddServiceAgent<TClient>(
        this IServiceCollection services,
        string baseUri,
        Action<LoggingOptions>? loggingConfigure = null,
        Action<HttpStandardResilienceOptions>? pollyConfigure = null)
        where TClient : class
    {
        services.TryAddSingleton<IRedactorProvider, NullRedactorProvider>();
        var builder = services.AddHttpClient<TClient>(h =>
        {
            h.BaseAddress = new Uri(baseUri);
            h.AddCqrsAcceptHeaders();
        });
        builder.AddLogging(loggingConfigure);
        builder.ApplyResilienceConfigure(pollyConfigure);
        return builder;
    }

    /// <summary>
    ///     Inject a service agent to services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="baseUri">The base uri for api.</param>
    /// <param name="loggingConfigure">Configure logging behavior.</param>
    /// <param name="pollyConfigure">The polly policy for underlying httpclient.</param>
    /// <typeparam name="TClient">The type of api client.</typeparam>
    /// <typeparam name="TImplementation">The type of service agent</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddServiceAgent<TClient, TImplementation>(
        this IServiceCollection services,
        string baseUri,
        Action<LoggingOptions>? loggingConfigure = null,
        Action<HttpStandardResilienceOptions>? pollyConfigure = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.TryAddSingleton<IRedactorProvider, NullRedactorProvider>();
        var builder = services.AddHttpClient<TClient, TImplementation>(h =>
        {
            h.BaseAddress = new Uri(baseUri);
            h.AddCqrsAcceptHeaders();
        });
        builder.AddLogging(loggingConfigure);
        builder.ApplyResilienceConfigure(pollyConfigure);
        return builder;
    }

    private static void AddLogging(this IHttpClientBuilder h, Action<LoggingOptions>? configure = null)
    {
        h.AddExtendedHttpClientLogging(o =>
        {
            o.LogBody = false;
            o.LogRequestStart = false;
            o.BodySizeLimit = 2000;
            configure?.Invoke(o);
        });
    }

    private static void AddCqrsAcceptHeaders(this HttpClient h)
    {
        h.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
        h.DefaultRequestHeaders.AppendCurrentCqrsVersion();
    }

    private static void ApplyResilienceConfigure(
        this IHttpClientBuilder builder,
        Action<HttpStandardResilienceOptions>? extraConfigure)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.DisableForUnsafeHttpMethods();
            options.Retry.MaxRetryAttempts = 3;
            extraConfigure?.Invoke(options);
        });
    }
}
