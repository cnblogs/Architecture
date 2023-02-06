using System.Reflection;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// 用于事件订阅的扩展方法。
/// </summary>
public static class EndPointExtensions
{
    /// <summary>
    ///     订阅集成事件，默认路径是 /subscriptions/{AppName}/events/{EventName}。AppName 将由事件所属 Assembly 中的元数据指定。
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/></param>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <returns><see cref="IEndpointConventionBuilder"/></returns>
    public static IEndpointConventionBuilder Subscribe<TEvent>(this IEndpointRouteBuilder builder)
        where TEvent : IntegrationEvent
    {
        var attr = typeof(TEvent).Assembly
            .GetCustomAttributes(typeof(AssemblyAppNameAttribute), false)
            .Cast<AssemblyAppNameAttribute>()
            .FirstOrDefault();
        if (attr is null || string.IsNullOrEmpty(attr.Name))
        {
            throw new InvalidOperationException(
                $"No AppName was configured in assembly for event: {typeof(TEvent).Name}, either use Subscribe<TEvent>(string appName) method to set AppName manually or add [assembly:AssemblyAppName()] to the Assembly that {typeof(TEvent).Name} belongs to");
        }

        return builder.Subscribe<TEvent>(attr.Name);
    }

    /// <summary>
    /// 订阅集成事件，默认路径是 /subscriptions/{AppName}/events/{EventName}。
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/>。</param>
    /// <param name="appName">事件隶属名称。</param>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder Subscribe<TEvent>(this IEndpointRouteBuilder builder, string appName)
        where TEvent : IntegrationEvent
    {
        var eventName = typeof(TEvent).Name;
        return builder.Subscribe<TEvent>("/subscriptions/" + appName + "/events/" + eventName, appName);
    }

    /// <summary>
    /// 订阅集成事件。
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/>。</param>
    /// <param name="route">事件访问路径。</param>
    /// <param name="appName">应用名称。</param>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder Subscribe<TEvent>(
        this IEndpointRouteBuilder builder,
        string route,
        string appName)
        where TEvent : IntegrationEvent
    {
        builder.EnsureDaprEventBus();

        var result = builder
            .MapPost(route, (TEvent receivedEvent, IEventBus eventBus) => eventBus.ReceiveAsync(receivedEvent))
            .WithTopic(DaprOptions.PubSubName, DaprUtils.GetDaprTopicName<TEvent>(appName));
        return result;
    }

    /// <summary>
    ///     订阅 Assembly 中的全部事件。
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="assemblies"><see cref="Assembly"/></param>
    public static void Subscribe(this IEndpointRouteBuilder builder, params Assembly[] assemblies)
    {
        builder.EnsureDaprEventBus();

        var method = typeof(EndPointExtensions).GetMethod(
            nameof(Subscribe),
            new[] { typeof(IEndpointRouteBuilder), typeof(string) })!;
        foreach (var assembly in assemblies)
        {
            var events = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(IntegrationEvent))).ToList();
            var attr = assembly
                .GetCustomAttributes(typeof(AssemblyAppNameAttribute), false)
                .Cast<AssemblyAppNameAttribute>()
                .FirstOrDefault();
            if (attr is null || string.IsNullOrEmpty(attr.Name))
            {
                throw new InvalidOperationException(
                    $"No AppName was configured in assembly: {assembly.FullName}, either use Subscribe<TEvent>(string appName) method to set AppName manually or add [assembly:AssemblyAppName()] to the Assembly");
            }

            events.ForEach(e => method.MakeGenericMethod(e).Invoke(null, new object[] { builder, attr.Name }));
        }
    }

    private static void EnsureEventBusRegistered(this IEndpointRouteBuilder builder, DaprOptions daprOptions)
    {
        if (daprOptions.IsEventBusRegistered)
        {
            return;
        }

        var serviceCheck = builder.ServiceProvider.GetRequiredService<IServiceProviderIsService>();
        if (!serviceCheck.IsService(typeof(IEventBus)))
        {
            throw new InvalidOperationException(
                $"{nameof(IEventBus)} has not been registered. Did you forget to call IServiceCollection.AddDaprEventBus()?");
        }

        daprOptions.IsEventBusRegistered = true;
        return;
    }

    private static void EnsureDaprSubscribeHandlerMapped(this IEndpointRouteBuilder builder, DaprOptions daprOptions)
    {
        if (daprOptions.IsDaprSubscribeHandlerMapped)
        {
            return;
        }

        if (builder is IApplicationBuilder app)
        {
            app.UseCloudEvents();
        }

        builder.MapSubscribeHandler();
        daprOptions.IsDaprSubscribeHandlerMapped = true;
        return;
    }

    private static DaprOptions GetDaprOptions(this IEndpointRouteBuilder builder)
        => builder.ServiceProvider.GetRequiredService<IOptions<DaprOptions>>().Value;

    private static void EnsureDaprEventBus(this IEndpointRouteBuilder builder)
    {
        var options = builder.GetDaprOptions();
        builder.EnsureDaprSubscribeHandlerMapped(options);
        builder.EnsureEventBusRegistered(options);
    }
}