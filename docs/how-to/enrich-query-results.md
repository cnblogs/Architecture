# How-to：用 Enricher 聚合外部数据

**场景**：查询结果里有一部分字段来自聚合之外——另一个微服务、另一个数据库、外部 API。不想在查询 Handler 里直接调外部服务，也不想为此把多个数据源搅在一个查询里。

## 模式

Enricher 把"取外部数据并回填 DTO"从查询 Handler 中解耦出来：

- 查询 Handler 只负责本聚合的数据，DTO 上外部字段留空。
- 框架管道在 Handler 返回后，自动对结果调用匹配的 `IEnricher<T>` 补全。
- 每个 Enricher 单一职责，多个 Enricher 可叠加，可独立测试。

## 1. DTO 实现 IModel，查询用 Model 变体

```csharp
public class CorporationOrderDto : IModel
{
    public long Id { get; set; }
    public int PayStatus { get; set; }
    // 以下字段来自订购服务，本服务查询留空
    public string? ProductName { get; set; }
    public decimal? Amount { get; set; }
}

public record ListCorporationOrdersQuery(/*...*/) : IPageableModelQuery<CorporationOrderDto>;
```

## 2. 实现 IEnricher<T>

```csharp
public class CorporationOrderEnricher(IOrderingClient orderingClient, ILogger<CorporationOrderEnricher> logger)
    : IEnricher<CorporationOrderDto>
{
    // 纯 HTTP 调用，允许并行
    public bool AllowParallel => true;

    public Task EnrichAsync(CorporationOrderDto model, CancellationToken ct) => BulkEnrichAsync([model], ct);

    public async Task BulkEnrichAsync(IEnumerable<CorporationOrderDto> models, CancellationToken ct)
    {
        var targets = models.Where(m => string.IsNullOrEmpty(m.ProductName)).ToList();  // 已填充的跳过
        if (targets.Count == 0)
            return;

        // 收集 Id → 一次批量调用 → 字典回填，避免 N+1
        var orders = await orderingClient.BatchGetOrdersAsync(targets.Select(m => m.OrderId).ToList(), ct);
        var dict = orders.ToDictionary(o => o.OrderId);
        foreach (var model in targets)
        {
            if (dict.TryGetValue(model.OrderId, out var order))
            {
                model.ProductName = order.ProductName;
                model.Amount = order.Amount;
            }
            else
            {
                logger.LogInformation("订购服务中找不到订单 {OrderId}，跳过补全", model.OrderId);
            }
        }
    }
}
```

`AllowParallel` 的取舍：

| 场景 | 取值 |
| --- | --- |
| 纯 HTTP 调用（ServiceAgent、第三方 API） | `true`，单条补全可并行 |
| 内部执行 EF Core 查询 | `false`——并行调用同一 DbContext 有线程安全问题 |

EF 场景的写法（合同文件补全）：

```csharp
public class CorporationContractFileEnricher(CorporationContractRepository repository)
    : IEnricher<CorporationContractFileDto>
{
    // Enricher 内部执行 EF Core 查询，并行调用会有 DbContext 线程安全问题
    public bool AllowParallel => false;

    public async Task BulkEnrichAsync(IEnumerable<CorporationContractFileDto> models, CancellationToken ct)
    {
        var ids = models.Select(m => m.FileId).Distinct().ToList();
        var files = await repository.GetNoTrackingQueryable<CorporationFile>()
            .Where(f => ids.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, ct);
        foreach (var model in models)
        {
            if (files.TryGetValue(model.FileId, out var file))
                model.FileName = file.Name;
        }
    }
}
```

## 3. 注册

```csharp
services.AddCqrs(typeof(ListCorporationOrdersQuery).Assembly)
    .AddEnrichers();   // 扫描程序集注册所有 IEnricher<T>，无需逐个注册
```

之后所有返回 `CorporationOrderDto`（单条、列表、分页、命令返回体）的请求都会自动补全——`PagedList<T>`、字典容器等集合形态框架会自动展开。

## 4. 多个 Enricher 的顺序与共用

- 同一 DTO 有多个 Enricher 时默认并行执行；存在依赖（B 需要 A 补全的字段）时给 B 标注：

```csharp
[EnrichAfter(typeof(EnrichableModelEnricher))]
public class RunAfterEnricher : IEnricher<IEnrichableModel> { /* ... */ }
```

先确认真的需要——多一级顺序会增加理解成本。

- 多个 DTO 共用一套补全逻辑时，目标类型用接口（框架自动展开到所有实现）：

```csharp
public interface IGeoLocatable
{
    string? Ip { get; }
    string? Province { get; set; }
}

public class GeoIpEnricher(YuanDunAgent agent) : IEnricher<IGeoLocatable> { /* ... */ }
```

## 5. 与缓存的关系

`AddHybridQueryCache()` 在 `AddEnrichers()` **之前**调用时，Enricher 的结果会写入缓存（外部调用省掉）；之后调用则每次命中缓存后再补全。详见 [cache-queries](cache-queries.md)。

## 什么时候不用 Enricher

- 外部数据是查询的**过滤/排序条件**——Enricher 发生在查询之后，必须把条件下推到查询本身（如在查询里联另一个仓储的 `NoTrackingQueryable`）。
- 只是本聚合内导航属性的投影——直接用 Mapster 配置（见 [implement-query](implement-query.md)）。
