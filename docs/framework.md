# 框架指南（自底向上）

本文自底向上介绍 Cnblogs.Architecture 各层包提供的能力与推荐用法。如果希望按任务查找做法（"如何实现一个命令"、"如何加缓存"），请从侧边栏的 How-to 系列文档开始，文末也有[任务导航](#任务导航)。

- [包总览](#包总览)
- [解决方案分层布局](#解决方案分层布局)
- [Domain 层](#domain-层)
- [Infrastructure 层](#infrastructure-层)
- [Application 层（CQRS）](#application-层cqrs)
- [DI 组装](#di-组装)
- [API 层](#api-层)
- [ServiceAgent](#serviceagent)
- [集成事件（EventBus）](#集成事件eventbus)
- [AgentFramework（AI 工具集成）](#agentframeworkai-工具集成)
- [任务导航](#任务导航)

## 包总览

| 包 | 职责 |
| --- | --- |
| `Cnblogs.Architecture.Ddd.Domain.Abstractions` | 实体、聚合根、领域事件、仓储接口、ID/时间/随机数提供器 |
| `Cnblogs.Architecture.Ddd.Infrastructure.Abstractions` | 分页/排序模型、缓存/锁/文件提供器抽象、HybridCache 默认实现 |
| `Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework` | EF Core 版 `BaseRepository`（含领域事件派发） |
| `Cnblogs.Architecture.Ddd.Infrastructure.CacheProviders.Redis` | 基于 StackExchange.Redis 的混合缓存 L2 |
| `Cnblogs.Architecture.Ddd.Infrastructure.RedLock` | 基于 RedLock.net 的分布式锁 |
| `Cnblogs.Architecture.Ddd.Infrastructure.FileProviders.AliyunOss` | 阿里云 OSS 文件提供器 |
| `Cnblogs.Architecture.Ddd.Infrastructure.Dapper` / `.Dapper.Clickhouse` | Dapper 上下文抽象与 ClickHouse 模型映射 |
| `Cnblogs.Architecture.Ddd.Infrastructure.MongoDb` | MongoDB 上下文与仓储 |
| `Cnblogs.Architecture.Ddd.Cqrs.Abstractions` | 命令/查询接口、管道行为（校验/缓存/锁/Enricher）、`CommandResponse` |
| `Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection` | `AddCqrs()` 及一系列链式注册扩展 |
| `Cnblogs.Architecture.Ddd.Cqrs.EntityFramework` | EF 版分页查询处理器基类 |
| `Cnblogs.Architecture.Ddd.Cqrs.Dapper` / `.Dapper.Clickhouse` / `.Dapper.SqlServer` | Dapper 上下文注册扩展 |
| `Cnblogs.Architecture.Ddd.Cqrs.MongoDb` / `.MongoDb.Analyzer` | Mongo 上下文注册、Mongo LINQ 分析器 |
| `Cnblogs.Architecture.Ddd.Cqrs.AspNetCore` | Minimal API 端点映射、错误响应约定、API 版本 |
| `Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent` | 强类型 HTTP 客户端基类 |
| `Cnblogs.Architecture.ServiceAgent.Design` | dotnet-cnb 生成 ServiceAgent 所需的设计时导出器 |
| `Cnblogs.Architecture.Ddd.EventBus.Abstractions` / `.Dapr` | 集成事件、内存缓冲、Dapr pub/sub |
| `Cnblogs.Architecture.Ddd.Cqrs.AgentFramework` | 将命令/查询暴露为 AI 工具（Microsoft Agent Framework） |

## 解决方案分层布局

设计上将服务分为四层，从内到外分别是 Domain、Infrastructure、Application 和 API/Web/ServiceAgent，外层依赖内层，内层不依赖外层。

一个典型服务的项目布局如下：

```
src/
  MyApp.Domain.ValueObjects/     # 纯枚举（继承 Enumeration），被 Domain/Dto/Payloads 共享
  MyApp.Domain/                  # 实体、领域事件、仓储接口（不含 EF 等基础设施依赖）
  MyApp.Application.Dto/         # 查询返回的 DTO 与错误码（MyAppError : Enumeration）
  MyApp.IntegrationEvents/       # 对外发布的集成事件契约（供消费方 NuGet 引用）
  MyApp.Api.Payloads/            # HTTP 请求 Payload（ServiceAgent 契约的一部分）
  MyApp.Application/             # Command/Query/Handler、领域事件处理器、Enricher
  MyApp.Infrastructure/          # DbContext、EF 配置、仓储实现、迁移
  MyApp.Bootstrapper/            # Api 与 Web 共用的 DI 组装层
  MyApp.Api/                     # Minimal API 主机
  MyApp.Web/                     # 控制器风格主机（可选，内网后台）
  MyApp.ServiceAgent/            # 由 dotnet-cnb 生成的客户端 NuGet 包
test/
  MyApp.UnitTests/
```

把 ValueObjects、Dto、Payloads、IntegrationEvents 拆成独立项目的目的：让 ServiceAgent 包可以只引用这些轻量契约类型，避免把 Application/Domain 拖进客户端项目。ServiceAgent 的 csproj 通过 `PrivateAssets="all"` 的 ProjectReference + `CopyProjectReferencesToPackage` Target 把这些契约程序集内嵌进 nupkg。

## Domain 层

包：`Cnblogs.Architecture.Ddd.Domain.Abstractions`

### 实体与聚合根

```csharp
public class FakeBlog : Entity<int>, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
}
```

- `Entity<TKey>`：继承自 `Entity`，带 `TKey Id`。推荐所有新实体都继承它。
- `Entity`：自带 `Deleted`（软删除标记）、`DateAdded`、`DateUpdated`，更新前会自动刷新 `DateUpdated`。仓储的 `DeleteAsync` / `DeleteRangeAsync` 默认软删除（置 `Deleted = true`，传 `softDelete: false` 执行硬删除），查询过滤需要在 EF 配置中自行添加 `HasQueryFilter(x => x.Deleted == false)`。
- `EntityBase`：无默认属性的最小基类，仅用于兼容存量表；新实体不要使用。
- `Entity<TKey>.AddDomainEvent(Func<TKey, IDomainEvent> generator)`：延迟生成领域事件——在实体尚未落库、还没有 Id 时使用，事件在读取 `DomainEvents` 时才用实际 Id 构造。
- `IAggregateRoot`：空标记接口，仓储接口约束聚合根。
- `Enumeration`：智能枚举基类，`Name` + `int Id`，提供 `FromValue<T>()`、`FromDisplayName<T>()`、`GetAll<T>()`。错误码也用它定义。

### 领域事件

```csharp
public record ChildAddedEvent(int ParentId, int ChildId) : DomainEvent;
```

- `DomainEvent` 是 record，自带 `CreateAt` 时间戳；实现 `IDomainEvent`（即 MediatR 的 `INotification`）。
- 聚合内部通过 `AddDomainEvent(...)` 记录事件，仓储保存时自动派发给 `IDomainEventHandler<T>` 处理器。

### 仓储接口

| 接口 | 适用场景 |
| --- | --- |
| `IRepository<TEntity, TKey>` | 基础 CRUD 与批量 API：`GetRangeAsync(keys)` 按主键批量获取，`AddRangeAsync` / `UpdateRangeAsync` / `DeleteRangeAsync` 批量写入；`DeleteAsync` / `DeleteRangeAsync` 默认软删除，传 `softDelete: false` 才执行硬删除；`NoTrackingQueryable` 供 Handler 中的只读查询使用 |
| `INavigationRepository<TEntity, TKey>` | 额外支持带 `includes` 的 `GetAsync(key, includes)` 与 `GetRangeAsync(keys, includes)`（强类型表达式或字符串路径），按需加载导航属性（支持过滤导航属性） |
| `ISqlRepository<TEntity, TKey>` | 额外支持原生 SQL（`SqlQuery` / `ExecuteSqlAsync`） |
| `IUnitOfWork<TEntity, TKey>` | 手动控制提交时机；`Add/Update/Delete` 仅暂存，`SaveEntitiesAsync()` 提交并派发领域事件 |

写操作（`AddAsync` / `UpdateAsync` / `DeleteAsync` 等）都会立即提交并派发领域事件；聚合内部多处修改天然落到同一事务的推荐做法是：一次 `GetAsync` 加载聚合（含导航），调用聚合方法完成所有修改，再一次 `UpdateAsync`。

### 提供器

| 接口 | 说明 |
| --- | --- |
| `IDateTimeProvider` | 可替换的时间源，含 `Today()`、`EndOfYesterday()` 等便捷方法；测试时打桩 |
| `IRandomProvider` | 可替换的随机数源 |
| `IIdProvider` | `NextReadable()` 生成人类可读的 `yyyyMMddHHmmssXXXYYY` Id（XXX 为实例位，每实例每秒最多约 100 个）；`NextNumeric()` 生成雪花 Id（epoch 2020-01-01）。用于 `long` 主键或业务单号 |

配套扩展方法 `DateTimeOffsetCalculator.StartOfTheDay/EndOfTheDay/StartOfTheWeek/...` 用于日期计算。

## Infrastructure 层

### EF Core（Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework）

仓储实现只需一行：

```csharp
public class BlogRepository(IMediator mediator, BlogDbContext context)
    : BaseRepository<BlogDbContext, Blog, int>(mediator, context), IBlogRepository;
```

`BaseRepository<TContext, TEntity, TKey>` 的行为要点：

- `GetAsync(key, includes)` 与 `GetRangeAsync(keys, includes)` 支持强类型 Include（可对导航属性使用 `Where` 过滤）和字符串路径（`"Posts.Tags"`）两种形式。
- `AddAsync/UpdateAsync/DeleteAsync` 先 `SaveChangesAsync`，再通过注入的 `IMediator` 派发领域事件。
- `DeleteAsync` / `DeleteRangeAsync` 默认软删除：把 `Deleted` 置为 `true` 并保存 UPDATE（要求实体继承 `Entity`，否则抛 `InvalidOperationException`；未跟踪的实体会被自动 Attach）。传 `softDelete: false` 才执行 DELETE。两种方式保存后都会解除跟踪，行为一致。
- `UnitOfWork.SaveChangesAsync()` 只提交不派发事件；`SaveEntitiesAsync()` 先派发事件再提交。
- `protected virtual Task BeforeDispatchDomainEventAsync(...)` 是派发前的钩子，可用于事件持久化（outbox）或日志。
- `QueryableExtensions.AggregateIncludes(...)` 将 Include 列表串成查询。
- `EntityConfigurationExtensions.IgnoreEntityDefaultProperties<T>()`：当实体对应的表没有 `Deleted/DateAdded/DateUpdated` 列时忽略这三个属性。

注意：框架不强制数据库外键，EF 生成的脚本交给 DBA 应用时也会去掉外键约束。

### 缓存（Infrastructure.Abstractions + CacheProviders.Redis）

`ICacheProvider` 是缓存门面，默认实现 `HybridCacheProvider` 建立在 `Microsoft.Extensions.Caching.Hybrid`（HybridCache）之上：

```csharp
Task<TResult> GetOrCreateAsync<TResult>(
    string cacheKey,
    Func<CancellationToken, ValueTask<TResult>> factory,
    TimeSpan? remoteExpires = null,
    TimeSpan? localExpires = null,
    string[]? groupNames = null,
    CancellationToken cancellationToken = default);
```

应用代码一般不直接调用它，而是给查询实现 `ICachableRequest`（见 [Application 层](#application-层cqrs) 与 [How-to：查询缓存](how-to/cache-queries.md)）。

### 分布式锁（Infrastructure.RedLock）

`IDistributedLockProvider.ExecuteWithLockAsync(key, action, expiresIn)`，获取失败抛 `AcquireDistributionLockFailedException`。`AddRedLockDistributionLock(config)` 基于 RedLock.net + Redis。应用代码一般通过 `ILockableRequest` 使用（见下文）。

### 文件（Infrastructure.Abstractions + FileProviders.AliyunOss）

`IFileProvider`（读写删文件流/字节）与 `IFileDeliveryProvider`（生成限时下载链接）。默认实现 `DefaultFileProvider` 直接访问本地文件系统；`AddAliyunOssFileProviders(config)` 注册阿里云 OSS 版本（AccessKeyId 留空时回退到 ECS RAM 角色）。

### Dapper 与 ClickHouse

当查询超出 EF 的表达范围（重报表、跨库、超大 IN 列表）时可以走 Dapper 逃生舱：

```csharp
services.AddDapperContext<AnalyticsDapperContext>()
    .UseSqlServer(connectionString);          // 或 .UseClickhouse(connectionString)
```

继承 `DapperContext`，注入后 `CreateDbConnection()` 即可用原生 Dapper。ClickHouse 场景继承 `ClickhouseDapperContext`，用 `ConfigureModels` 声明实体到表的映射后，`BulkWriteAsync<T>()` 可批量写入。

### MongoDB

`AddMongoContext<TContext>(connectionString, databaseName)` 注册 Mongo 上下文与 `MongoBaseRepository`（与 EF 仓储同构，含领域事件与事务性批量写）。安装 `Cnblogs.Architecture.Ddd.Cqrs.MongoDb.Analyzer` 可获得预配置为 LINQ3 的 Mongo LINQ 分析器。

### 分页与排序模型（Infrastructure.Abstractions）

- `PagedList<T>`：`Items` + `PageIndex` + `PageSize` + `TotalCount`，是分页查询的标准返回类型。
- `PagingParams(PageIndex, PageSize)`：Minimal API 中自动从路由或查询字符串绑定 `pageIndex`/`pageSize`。
- `OrderBySegmentConfig.RegisterSortableProperty<TSource, TProperty>(name, lambda)`：注册排序白名单；`OrderByString`（如 `"-dateAdded,id"`）解析时忽略未注册的名字。
- `System.Linq.QueryOrderer` / `QueryPager`：`IQueryable` 的排序与分页扩展。

## Application 层（CQRS）

包：`Cnblogs.Architecture.Ddd.Cqrs.Abstractions`

### 请求类型

| 接口 | 返回 | 用途 |
| --- | --- | --- |
| `ICommand<TError>` | `CommandResponse<TError>` | 无返回体的命令 |
| `ICommand<TView, TError>` | `CommandResponse<TView, TError>` | 有返回体的命令 |
| `IQuery<TView>` | `TView?`（找不到为 null → HTTP 404） | 单项查询 |
| `IListQuery<TList>` | `TList` | 小型不分页列表（如省市、地址） |
| `IOrderedQuery<TList>` | `TList` + `OrderByString` | 支持排序的列表 |
| `IPageableQuery<TElement>` | `PagedList<TElement>` | 分页列表 |
| `IPageableModelQuery<TModel>` | `PagedList<TModel>` | 同上，DTO 实现 `IModel` 标记接口，配合 `EfPageableModelQueryHandler` 与 Enricher 使用 |

`ICommand` 自带 `bool ValidateOnly` 干跑标志。`TError` 是 `Enumeration` 子类，集中定义该聚合的业务错误码。

### CommandResponse 与错误码

```csharp
public class InvoiceError : Enumeration
{
    public static readonly InvoiceError ItemAlreadyInvoiced = new(1, "商品已在其他发票中");
    public static readonly InvoiceError InvalidStatus = new(2, "发票状态不正确");

    [method: JsonConstructor]
    private InvoiceError(int id, string name) : base(id, name) { }
}
```

`CommandResponse` 家族（`CommandResponse` / `CommandResponse<TError>` / `CommandResponse<TView, TError>`）提供 `Success()` / `Success(view)` / `Fail(errorCode)` 静态工厂；`IsSuccess()` 判断成败；`ErrorCode` 携带错误码。HTTP 侧自动翻译为 200/204/400/429（见 [API 层](#api-层)）。

### 管道行为

`AddCqrs()` 默认注册 `ValidationBehavior` 与 `LoggingBehavior`，其他行为按需追加：

| 行为 | 触发条件 | 效果 |
| --- | --- | --- |
| `ValidationBehavior` | 请求实现 `IValidatable` | 调用 `Validate(ValidationErrors)`，有错误则直接返回 400，Handler 不执行 |
| `LoggingBehavior` | 所有请求 | Debug 级记录请求进入/完成 |
| `CacheableRequestBehavior` | 请求实现 `ICachableRequest`（需 `AddHybridQueryCache()`） | 按 `CacheKey()` 走 HybridCache；本地/远端行为与过期时间由请求自己声明 |
| `LockableRequestBehavior` | 请求实现 `ILockableRequest`（需注册锁提供器） | 分布式锁包裹 Handler；抢锁失败返回 `IsConcurrentError = true`（HTTP 429） |
| `EnricherBehavior` | 请求实现 `IEnrichableRequest`（即所有 Query/ICommand<TView,TError>，需 `AddEnrichers()`） | Handler 返回后对结果逐个/批量调用匹配的 `IEnricher<T>` 补全外部数据 |

### 校验：IValidatable

静态格式校验（无需查库）放在命令上，框架自动执行并返回 400：

```csharp
public record AddEntityCommand(string Name, bool ValidateOnly = false)
    : ICommand<EntityError>, IValidatable
{
    public void Validate(ValidationErrors errors)
    {
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(new ValidationError("分类名称不能是空的", nameof(Name)));
    }
}
```

需要查库的逻辑校验放在 Handler 里，失败返回 `CommandResponse.Fail(errorCode)`。

### 缓存：ICachableRequest

查询实现 `ICachableRequest` 后自动走缓存：

- `LocalCacheBehavior` / `RemoteCacheBehavior`（`UpdateCacheIfMiss` 或 `DisabledCache`）
- `LocalExpires`（秒级，如 15s）/ `RemoteExpires`（分钟级以上）
- `CacheGroupKey()` / `CacheGroupKeys()`：缓存分组；`GetCacheKeyParameters()`：缓存键组成，默认以 `-` 连接小写化。
- 失效：`InvalidCacheRequest(request[, InvalidWholeGroup])` 精确/整组失效；`InvalidCacheGroupsRequest(groupKeys)` 按组名失效（注意类型名是复数 Groups）。通常在领域事件处理器中发送。

### 锁：ILockableRequest

命令实现 `GetLockKey()`（可选 `IConfigurableLockableRequest.ExpiresIn` 指定过期），配合 `AddDistributionLock<TProvider>()`（或 `AddRedLockDistributionLock(config)`）即可防并发重复执行，失败自动映射 429。

### Enricher

查询结果需要聚合外的数据（另一个服务、另一个数据库）时，定义 `IEnricher<T>`：

```csharp
public class OrderEnricher(IOrderingClient client) : IEnricher<OrderDto>
{
    public bool AllowParallel => true;   // HTTP 调用可并行；EF 查询必须 false
    public async Task EnrichAsync(OrderDto model, CancellationToken ct) { /* 单个补全 */ }
    public async Task BulkEnrichAsync(IEnumerable<OrderDto> models, CancellationToken ct)
    {
        // 收集 Id → 一次批量调用 → 字典回填，避免 N+1
    }
}
```

详见 [How-to：Enricher](how-to/enrich-query-results.md)。

### 分页查询基类

EF 分页查询继承 `EfPageableQueryHandler<TQuery, TEntity, TView>`（Model 变体为 `EfPageableModelQueryHandler`），只需实现 `Filter(query)` 与 `DefaultOrderBy(query, queryable)`。基类负责：解析 `OrderByString`、`CountAsync`、`Paging()`、Mapster `ProjectToType<TView>()`、`ToListAsync()`。两个内建技巧：

- `PageSize = 0`：只查 `TotalCount` 不取数据——统计数量不需要单独的查询。
- 负数 `PageIndex`：按默认排序的相反方向取页（配合 `DefaultReverseOrderBy` 虚方法）。

## DI 组装

`AddCqrs(params Assembly[])` 扫描程序集注册 MediatR 处理器，返回 `CqrsInjector` 支持链式追加：

```csharp
builder.Services.AddCqrs(typeof(GetArticleQuery).Assembly, typeof(CreateArticleCommand).Assembly)
    .AddLongToStringJsonConverter()
    .AddDefaultIdProvider(0)
    .AddEventBus(o => o.UseDapr("myapp"))
    .AddHybridQueryCache()
    .AddEnrichers()
    .AddRedLockDistributionLock(builder.Configuration)
    .AddDefaultDateTimeAndRandomProvider();
```

| 扩展方法 | 作用 |
| --- | --- |
| `AddDefaultDateTimeAndRandomProvider()` / `AddDateTimeProvider<T>()` / `AddRandomProvider<T>()` | 注册时间/随机数提供器 |
| `AddDefaultIdProvider([instanceId])` / `AddIdProvider<T>()` | 注册 Id 提供器 |
| `AddHybridQueryCache()` | 启用查询缓存管道 + `InvalidCacheRequest` 处理器 + HybridCache（L1 内存；注册了 `IDistributedCache` 实现时自动作为 L2） |
| `AddRedisQueryCache(connectionStringOrConfig)` | 注册 StackExchange.Redis 分布式缓存并启用混合查询缓存（L1 + L2 一步到位） |
| `AddEnrichers()` | 扫描注册 `IEnricher<T>` 与 Enricher 管道 |
| `AddDistributionLock<T>()` / `AddRedLockDistributionLock(config)` | 启用分布式锁管道 |
| `AddDefaultFileProvider()` / `AddFileProvider<T>()` / `AddFileDeliveryProvider<T>()` | 文件提供器 |
| `AddEventBus(o => ...)` | 集成事件总线（见下文） |
| `AddAgentFramework()` | AI 工具集成（见下文） |
| `UseProblemDetails()` / `UseCustomCommandErrorResponseMapper(...)` | 命令错误响应格式 |
| `AddLongToStringJsonConverter()` | `long` 序列化为 JSON 字符串（避免前端精度丢失） |

另外团队的 `Cnblogs.Redis` 包（`AddCnblogsRedis(config, section)`）用于注册命名 Redis 连接供业务直接使用，与框架缓存互不冲突。

## API 层

包：`Cnblogs.Architecture.Ddd.Cqrs.AspNetCore`

### Minimal API 映射

```csharp
var apis = app.NewVersionedApi();
var v1 = apis.MapGroup("/api/v{version:apiVersion}").HasApiVersion(1);

v1.MapQuery<ListStringsQuery>("strings");                                  // GET，分页参数自动绑定
v1.MapQuery<GetStringQuery>("apps/{appId}/strings/{stringId:int}/value");  // GET，404 语义自动处理
v1.MapCommand("articles", ([FromBody] CreateArticlePayload payload)
    => new CreateArticleCommand(payload.Title));                           // 动词由命令名推断
v1.MapPutCommand<IssueInvoiceCommand>("invoices/{invoiceId:long}:issue");  // 显式指定动词
v1.MapDeleteCommand<RemoveNoteCommand>("corporations/{corpId:int}/notes/{id:int}");
```

- `MapQuery<T>(route)`：`T` 经 `[AsParameters]` 从路由/查询字符串构造；返回 null 自动 404。`MapNullableRouteParameter.Enable` 会为可空路由参数生成 `-` 占位的路由变体（如 `apps/-/instance/-/roles`）；`enableHead: true` 额外生成 HEAD 端点。
- `MapCommand<TCommand>(route)`：按命令类型名前缀推断 HTTP 动词——`Create/Add/New` → POST，`Update/Modify/Replace/Alter` → PUT，`Delete/Remove/Clean/Clear/Purge` → DELETE，其余 PUT。可用 `MapPrefixToPost(...)` 等扩展调整前缀表。
- `MapPostCommand` / `MapPutCommand` / `MapDeleteCommand`：显式指定动词的形式，命令从路由/查询绑定；带请求体的场景用 Delegate 形式从 `[FromBody]` Payload 构造命令。
- 路由风格遵循 Google Cloud AIP：资源复数小写连字符（`entity-groups`）、自定义动词 camelCase 后缀（`:batchGet`、`:search`）、不适用时用 `-` 占位。

### 响应与错误约定

| 情况 | 结果 |
| --- | --- |
| 命令成功且无返回体 | 204 No Content |
| 命令成功且有返回体（`IObjectResponse`） | 200 + 返回体 JSON |
| `IValidatable` 校验失败 | 400（默认纯文本，`UseProblemDetails()` 换 ProblemDetails） |
| Handler 返回 `Fail(errorCode)` | 400 + 错误信息 |
| 分布式锁冲突（`IsConcurrentError`） | 429 |

客户端发送 `Accept: application/cqrs`（或 `X-Cqrs-Version: 2`）时，命令响应改用完整的 `CommandResponse` JSON 信封（含 `ErrorCode` 等字段），ServiceAgent 默认使用该协议。`CqrsHttpOptions` 支持自定义错误映射。

### 版本与其他

- `AddCnblogsApiVersioning()`：注册 API 版本（URL 段 `{version:apiVersion}` + 按命名空间约定 + OpenApi Explorer）。
- 控制器风格：`[ApiController]` 继承 `ApiControllerBase`，用 `HandleCommandResponse(response)` 得到与 Minimal API 一致的语义；`AddControllers().AddCqrsModelBinderProvider()` 让控制器也能绑定 `PagingParams`。内网管理后台可以用这种风格。
- `[Trimmed]` 特性：JSON 反序列化时自动去除字符串首尾空白。

## ServiceAgent

消费方通过生成的强类型客户端调用上述 API，无需手写 HttpClient。

服务端（API 项目）：

1. 引用 `Cnblogs.Architecture.ServiceAgent.Design`（仅生成时生效，运行时是 no-op）。
2. 安装全局工具 `dotnet tool install -g dotnet-cnb`。
3. 执行生成（多数项目根目录有 `generate-service-agent.ps1`）：

```bash
dotnet cnb serviceagent generate \
  --api-project ./src/MyApp.Api \
  --output ./src/MyApp.ServiceAgent \
  --namespace MyApp.ServiceAgent \
  --base-url http://myapp-api \
  --clean
```

生成原理与 EF 设计时工具一致：Design 包通过 HostingStartup 在应用启动时把真实端点导出为 manifest，工具再生成代码。产物三个文件：`IMyAppService.cs`（每个端点一个强类型方法，分页方法带 `pageIndex/pageSize/orderByString` 参数）、`MyAppService.cs`（继承 `CqrsServiceAgent<MyAppError>`）、`ServiceAgentExtensions.cs`（`AddMyAppService()`）。多版本 API 默认生成 `IMyAppV1Service`/`IMyAppV2Service`，`--api-version N` 可只生成指定版本。

消费方：

```csharp
services.AddServiceAgent<IMyAppService, MyAppService>("http://myapp-api");
```

`AddServiceAgent` 注册 Typed HttpClient，自动附加 `Accept: application/cqrs`、`X-Cqrs-Version: 2` 头，配置标准 resilience（安全方法重试最多 3 次）与可选日志。基类 `CqrsServiceAgent<TError>` 提供 `GetItemAsync` / `HasItemAsync` / `ListItemsAsync` / `ListPagedItemsAsync` / `BatchGetItemsAsync` / `Post/Put/DeleteCommandAsync`，自动处理 204/200/cqrs 信封三种响应协议。

## 集成事件（EventBus）

包：`Cnblogs.Architecture.Ddd.EventBus.Abstractions`（+ `.Dapr`）

```csharp
// 契约（MyApp.IntegrationEvents 项目）
[assembly: AssemblyAppName("myapp")]   // Dapr pub/sub 的 app 名，决定 topic 前缀

public record InvoiceIssuedIntegrationEvent(Guid Id, DateTimeOffset CreatedTime, long InvoiceId)
    : IntegrationEvent(Id, CreatedTime);
```

- 发布：领域事件处理器中注入 `IEventBus`，调用 `PublishAsync` / `TryPublishAsync`（后者吞掉并记录失败，不中断业务）。
- 事件先进进程内 `InMemoryEventBuffer`（上限防泄漏），`PublishIntegrationEventHostedService` 每秒批量经 Dapr pub/sub 发布到 topic `{appName}.{eventName}`；连续失败 5 次自动降级为每 60 秒重试，成功后恢复。
- 订阅：`app.Subscribe<OrderPaidIntegrationEvent>()`（或按程序集/处理器批量订阅），自动注册 `/subscriptions/{appName}/events/{eventName}` 端点；处理器实现 `IIntegrationEventHandler<TEvent>`，内部照常 `IMediator.Send` 命令。

详见 [How-to：集成事件](how-to/publish-integration-events.md)。

## AgentFramework（AI 工具集成）

包：`Cnblogs.Architecture.Ddd.Cqrs.AgentFramework`（基于 Microsoft.Extensions.AI + Microsoft.Agents.AI）

```csharp
builder.Services.AddCqrs(typeof(CreateArticleCommand).Assembly)
    .AddAgentFramework(o => o.Discovery = ToolDiscovery.Marked);

[AgentTool(Description = "Create an article with the given title.")]
public record CreateArticleCommand(string Title, bool ValidateOnly = false)
    : ICommand<ArticleError>;

public class ArticlesAgent : ICqrsAgent
{
    public string Name => "articles-agent";
    public string Instructions => "You manage articles.";
    public IReadOnlyList<Type>? AllowedRequestTypes => [typeof(CreateArticleCommand)];
}

app.MapAgent<ArticlesAgent>("/agents/articles");   // POST { "prompt": "..." }
```

标记了 `[AgentTool]` 的命令/查询会生成为 `AIFunction` 工具，AI 调用时走与 HTTP 相同的 MediatR 管道（校验/缓存/锁/Enricher照常生效）。`CqrsAgentOptions` 默认隐藏 `ValidateOnly`/`PagingParams` 等基础设施参数、分页上限 20 条；`AllowValidateOnly = true` 可把干跑参数暴露给模型。

## 任务导航

| 任务 | 文档 |
| --- | --- |
| 定义聚合根、子实体与领域事件 | [define-aggregate](how-to/define-aggregate.md) |
| 实现一个命令（校验、ValidateOnly、错误码、锁） | [implement-command](how-to/implement-command.md) |
| 实现查询（三件套、分页、排序、Mapster） | [implement-query](how-to/implement-query.md) |
| 暴露 HTTP API（路由风格、版本、错误响应） | [expose-api](how-to/expose-api.md) |
| 生成与使用 ServiceAgent | [setup-service-agent](how-to/setup-service-agent.md) |
| 给查询加缓存与失效 | [cache-queries](how-to/cache-queries.md) |
| 用 Enricher 聚合外部数据 | [enrich-query-results](how-to/enrich-query-results.md) |
| 发布/订阅集成事件 | [publish-integration-events](how-to/publish-integration-events.md) |
