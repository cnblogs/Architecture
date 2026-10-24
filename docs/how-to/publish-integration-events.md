# How-to：发布与订阅集成事件

**场景**：本服务的领域变化需要通知其他服务（跨服务最终一致），或需要消费其他服务的事件。

传输层是 Dapr pub/sub（RabbitMQ/Redis 等由集群侧组件配置决定），应用代码不感知具体中间件。

## 1. 定义契约（IntegrationEvents 项目）

```csharp
// MyApp.IntegrationEvents/InvoiceIssuedIntegrationEvent.cs
public record InvoiceIssuedIntegrationEvent(Guid Id, DateTimeOffset CreatedTime, long InvoiceId)
    : IntegrationEvent(Id, CreatedTime);

// MyApp.IntegrationEvents/AssemblyInfo.cs
[assembly: AssemblyAppName("invoice")]   // Dapr 侧的应用名，决定 topic 前缀 invoice.*
```

这个项目会打进 ServiceAgent nupkg 发布给消费方，消费方引用同一契约类型。

## 2. 从领域事件桥接到集成事件

在领域事件处理器里注入 `IEventBus` 发布。领域事件保证聚合内的即时一致性，集成事件负责对外通知：

```csharp
public class InvoiceIssuedDomainEventHandler(IEventBus eventBus)
    : IDomainEventHandler<InvoiceIssuedDomainEvent>
{
    public async Task Handle(InvoiceIssuedDomainEvent notification, CancellationToken ct)
        => await eventBus.TryPublishAsync(new InvoiceIssuedIntegrationEvent(
            Guid.NewGuid(), DateTimeOffset.Now, notification.InvoiceId));
}
```

`TryPublishAsync` 吞掉并记录发布失败，不中断业务——事件先进进程内缓冲，即使 Dapr 暂不可用也不会让开票接口报错。`PublishAsync` 则会把失败抛给调用方，仅在"发不出去就该失败"的场景使用。

## 3. 注册

```csharp
services.AddCqrs(/*...*/)
    .AddEventBus(o => o.UseDapr(typeof(InvoiceIssuedIntegrationEvent).Assembly));
    // 程序集带 [assembly: AssemblyAppName] 时 app 名自动取自特性；也可 UseDapr("invoice") 显式指定
```

发布链路的容错内建：事件先进入 `InMemoryEventBuffer`（有上限），后台服务每秒批量经 Dapr 发布到 topic `{appName}.{eventName}`；连续失败 5 次自动降级为 60 秒一次的重试节奏，成功一次即恢复。

## 4. 订阅方

处理器实现 `IIntegrationEventHandler<TEvent>`，放在 **Api/Web 层**（不是 Application 层——这里允许编排多个命令）：

```csharp
// 订单支付事件 → 更新本地订单状态 → 发货，两个命令依次编排
public class OrderPaidIntegrationEventHandler(IMediator mediator)
    : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    public async Task Handle(OrderPaidIntegrationEvent notification, CancellationToken ct)
    {
        if (notification.App != "corporation")
            return;   // 事件是广播的，先过滤属于自己的部分

        await mediator.Send(new UpdateCorporationOrderPayStatusCommand(notification.OrderId, /*...*/), ct);
        await mediator.Send(new ShipCorporationCommand(notification.OrderId), ct);
    }
}
```

在 Program.cs 订阅（自动注册 `/subscriptions/{appName}/events/{eventName}` 端点并接好 CloudEvents）：

```csharp
app.Subscribe<OrderPaidIntegrationEvent>();
// 或批量：app.Subscribe(typeof(OrderPaidIntegrationEvent).Assembly);
```

## 可靠性边界

- 集成事件是**至少一次投递**：订阅方处理器要幂等（重复的 `UpdatePayStatusCommand` 不产生副作用），命令里的逻辑校验天然承担这层防护。
- 发布端缓冲是进程内的：进程崩溃会丢失缓冲中未发出的事件。对不可丢的事件，在 `BaseRepository.BeforeDispatchDomainEventAsync` 钩子里做事件持久化（outbox）后再发布。
- 领域事件（进程内、Mediator 派发、强类型）与集成事件（跨进程、Dapr、最终一致）不要混用：聚合内联动用领域事件，跨服务通知用集成事件。
