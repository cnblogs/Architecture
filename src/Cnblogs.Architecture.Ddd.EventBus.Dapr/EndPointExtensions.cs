using System.Reflection;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

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
    public static IEndpointRouteBuilder Subscribe<TEvent>(this IEndpointRouteBuilder builder)
        where TEvent : IntegrationEvent
    {
        var appName = typeof(TEvent).Assembly.GetAppName();
        return builder.Subscribe<TEvent>(appName);
    }

    /// <summary>
    /// 订阅集成事件，默认路径是 /subscriptions/{AppName}/events/{EventName}。
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/>。</param>
    /// <param name="appName">事件隶属名称。</param>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <returns></returns>
    public static IEndpointRouteBuilder Subscribe<TEvent>(this IEndpointRouteBuilder builder, string appName)
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
    public static IEndpointRouteBuilder Subscribe<TEvent>(
        this IEndpointRouteBuilder builder,
        string route,
        string appName)
        where TEvent : IntegrationEvent
    {
        builder.EnsureDaprEventBus();

        builder
            .MapPost(route, (TEvent receivedEvent, IEventBus eventBus) => eventBus.ReceiveAsync(receivedEvent))
            .WithTopic(DaprOptions.PubSubName, DaprUtils.GetDaprTopicName<TEvent>(appName))
            .WithTags("Subscriptions");

        return builder;
    }

    /// <summary>
    ///     订阅 Assembly 中的全部事件。
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="assemblies"><see cref="Assembly"/></param>
    public static IEndpointRouteBuilder Subscribe(this IEndpointRouteBuilder builder, params Assembly[] assemblies)
    {
        builder.EnsureDaprEventBus();

        var method = GetSubscribeMethod();

        foreach (var assembly in assemblies)
        {
            var events = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(IntegrationEvent))).ToList();
            var appName = assembly.GetAppName();
            events.ForEach(e => method.InvokeSubscribe(e, builder, appName));
        }

        return builder;
    }

    /// <summary>
    /// Subscribes integration events that the TEventHandler implements
    /// </summary>
    /// <typeparam name="TEventHandler">The integration event handler that implements <![CDATA[IIntegrationEventHandler<TEvent>]]></typeparam>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/></param>
    public static IEndpointRouteBuilder SubscribeByEventHandler<TEventHandler>(this IEndpointRouteBuilder builder)
        where TEventHandler : IEventBusRequestHandler
    {
        return builder.SubscribeByEventHandler(typeof(TEventHandler));
    }

    /// <summary>
    /// Subscribes integration events that event handlers implement in assemblies
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="assemblies">assemblies that event handlers reside</param>
    /// <returns></returns>
    public static IEndpointRouteBuilder SubscribeByEventHandler(this IEndpointRouteBuilder builder, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                builder.SubscribeByEventHandler(type);
            }
        }

        return builder;
    }

    private static IEndpointRouteBuilder SubscribeByEventHandler(this IEndpointRouteBuilder builder, Type type)
    {
        var interfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

        foreach (var handlerInterface in interfaces)
        {
            var eventType = handlerInterface.GetGenericArguments().FirstOrDefault();
            if (eventType != null)
            {
                var assembly = eventType.Assembly;
                var appName = assembly.GetAppName();
                GetSubscribeMethod().InvokeSubscribe(eventType, builder, appName);
            }
        }

        return builder;
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
                $"{nameof(IEventBus)} has not been registered. Did you forget to call IServiceCollection.AddEventBus()?");
        }

        daprOptions.IsEventBusRegistered = true;
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
    }

    private static DaprOptions GetDaprOptions(this IEndpointRouteBuilder builder)
        => builder.ServiceProvider.GetRequiredService<IOptions<DaprOptions>>().Value;

    private static void EnsureDaprEventBus(this IEndpointRouteBuilder builder)
    {
        var options = builder.GetDaprOptions();
        builder.EnsureDaprSubscribeHandlerMapped(options);
        builder.EnsureEventBusRegistered(options);
    }

    private static MethodInfo GetSubscribeMethod()
    {
        return typeof(EndPointExtensions).GetMethod(
            nameof(Subscribe),
            [typeof(IEndpointRouteBuilder), typeof(string)])!;
    }

    private static void InvokeSubscribe(this MethodInfo method, Type eventType, IEndpointRouteBuilder builder, string appName)
    {
        method.MakeGenericMethod(eventType).Invoke(null, [builder, appName]);
    }

    private static string GetAppName(this Assembly assembly)
    {
        var appName = assembly
            .GetCustomAttributes(typeof(AssemblyAppNameAttribute), false)
            .Cast<AssemblyAppNameAttribute>()
            .FirstOrDefault()?.Name;

        if (string.IsNullOrEmpty(appName))
        {
            throw new InvalidOperationException(
                $"No AppName was configured in assembly: {assembly.FullName}, either use Subscribe<TEvent>(string appName) method to set AppName manually or add [assembly:AssemblyAppName()] to the Assembly");
        }

        return appName;
    }
}
