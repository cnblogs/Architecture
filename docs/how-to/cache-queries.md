# How-to：给查询加缓存与失效

**场景**：某个查询读多写少、允许短暂陈旧，希望框架托管缓存的读写与失效。

先确认真的需要缓存：缓存会增加失效逻辑与一致性窗口的维护成本。**默认不加**；只有用户明确提出、或同组请求已在缓存中时才实现。

## 1. 启用缓存管道

```csharp
builder.Services.AddCqrs(/*...*/)
    .AddRedisQueryCache(builder.Configuration, "Redis");   // L1 内存 + L2 Redis 一步到位
    // 或 AddHybridQueryCache() —— 仅 L1；DI 中有 IDistributedCache 实现时自动作为 L2
```

`ICachableRequest` 的行为与过期时间由**每个请求自己声明**，启用缓存管道对其他查询零影响。

## 2. 查询实现 ICachableRequest

```csharp
public record GetCorporationMemberByUserIdQuery(int? CorpId, Guid UserId)
    : IQuery<CorporationMemberDto?>, ICachableRequest
{
    // 缓存组：同一企业的成员查询一起失效
    public string? CacheGroupKey() => CacheGroups.Corporation(CorpId);

    // 缓存键：默认把参数小写化后以 '-' 连接
    public object?[] GetCacheKeyParameters()
        => [nameof(GetCorporationMemberByUserIdQuery), CorpId, UserId];

    public CacheBehavior LocalCacheBehavior => CacheBehavior.UpdateCacheIfMiss;
    public CacheBehavior RemoteCacheBehavior => CacheBehavior.UpdateCacheIfMiss;
    public TimeSpan? LocalExpires => TimeSpan.FromSeconds(15);
    public TimeSpan? RemoteExpires => TimeSpan.FromHours(1);
}
```

组名集中管理，避免字符串散落：

```csharp
public static class CacheGroups
{
    public static string Corporation(int? corpId) => $"corporation_{corpId ?? "not_specified"}";
}
```

取值建议：

| 设置 | 建议值 | 理由 |
| --- | --- | --- |
| `LocalCacheBehavior` / `RemoteCacheBehavior` | `UpdateCacheIfMiss` | 未命中回源并回填 |
| `LocalExpires` | 秒级（15s） | 本地缓存快速过期，缩小多实例间的窗口 |
| `RemoteExpires` | 分钟级以上（1h+） | 远端缓存扛主要流量 |
| 只想禁用某一层 | `DisabledCache` | 例如仅本地缓存、不走远端 |

两层都设为 `DisabledCache` 时该请求完全绕过缓存。

## 3. 在领域事件处理器中失效

谁改了数据谁负责失效。在对应领域事件的 `IDomainEventHandler` 里通过 `IMediator` 发送失效请求：

```csharp
public class CorpMemberCacheHandler(IMediator mediator)
    : IDomainEventHandler<CorpMemberConfigUpdatedDomainEvent>,
      IDomainEventHandler<CorporationShortNameUpdatedDomainEvent>
{
    public async Task Handle(CorpMemberConfigUpdatedDomainEvent e, CancellationToken ct)
    {
        // 精确失效一个键
        await mediator.Send(new InvalidCacheRequest(new GetCorporationMemberByUserIdQuery(e.CorpId, e.UserId)), ct);
    }

    public async Task Handle(CorporationShortNameUpdatedDomainEvent e, CancellationToken ct)
    {
        // 短名影响两个组（指定企业与未指定企业），整组失效
        await mediator.Send(new InvalidCacheGroupsRequest(
            [CacheGroups.Corporation(e.CorpId), CacheGroups.Corporation(null)]), ct);
    }
}
```

两种失效请求（注意 `InvalidCacheGroupsRequest` 是复数 Groups）：

- `InvalidCacheRequest(request)`：删除该请求 `CacheKey()` 对应的缓存；`InvalidCacheRequest(request, InvalidWholeGroup: true)` 同时删除其所在组。
- `InvalidCacheGroupsRequest(groupKeys)`：按组名批量删除。

失效失败默认只记日志不抛异常（不阻断业务）；确需失败感知时传 `ThrowIfFailed: true`。

## 4. 与 Enricher 的顺序

`AddHybridQueryCache()` 与 `AddEnrichers()` 的**调用顺序**决定 Enricher 结果是否被缓存：

```csharp
services.AddCqrs(/*...*/).AddHybridQueryCache().AddEnrichers();  // Enricher 结果写入缓存（推荐，外部调用省掉）
services.AddCqrs(/*...*/).AddEnrichers().AddHybridQueryCache();  // 每次命中缓存后再补全外部数据
```

若 Enricher 的数据比查询本身更易变（如实时价格），选择后者并给 Enricher 数据留出一致性窗口。
