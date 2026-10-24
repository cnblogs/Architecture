# Cnblogs.Architecture

`Cnblogs.Architecture` 是一系列用于快速构建 DDD + CQRS 微服务的 NuGet 包，基于 .NET 10，[发布在 nuget.org](https://www.nuget.org/packages?q=Cnblogs.Architecture)。它把实体与聚合、领域事件、命令/查询管道、HTTP 端点映射、服务间强类型调用等基础设施沉淀为可复用的组件，让你把精力集中在领域划分和业务逻辑上。

## 架构分层

设计上将服务分为四层，从内到外分别是 Domain、Infrastructure、Application 和 API/Web/ServiceAgent，外层依赖内层，内层不依赖外层：

- **Domain**：实体、领域事件及仓储接口
- **Infrastructure**：仓储实现以及各类基础设施适配（EF Core、Dapper、MongoDB、Redis、分布式锁、对象存储等）
- **Application**：业务逻辑（命令与查询）、领域事件处理器、订阅的集成事件处理器
- **API/Web/ServiceAgent**：提供 Web 服务和身份验证，根据路径调用 Application 层的对应命令和查询，尽量不包含业务逻辑

## 核心能力

- **DDD 基础设施**：`Entity<TKey>`/`IAggregateRoot` 基类与延迟领域事件；EF Core / Dapper / MongoDB 版仓储；软删除、智能枚举、可替换的 ID/时间/随机数提供器
- **CQRS 管道**：`AddCqrs()` 一行注册处理器，校验（`IValidatable`）、缓存（`ICachableRequest`）、分布式锁（`ILockableRequest`）、结果补全（Enricher）等行为按需叠加；`CommandResponse` + 错误码统一业务失败语义
- **Minimal API 集成**：`MapQuery`/`MapCommand` 把命令与查询映射为 Google Cloud AIP 风格的端点，自动处理 200/204/400/404/429 语义与 API 版本
- **ServiceAgent 代码生成**：`dotnet-cnb` 工具从真实端点导出契约，生成强类型 HTTP 客户端 NuGet 包，消费方不再手写 HttpClient
- **集成事件**：基于 Dapr pub/sub 的跨服务事件，进程内缓冲、批量发布、失败自动降级重试
- **AI 工具集成**：`[AgentTool]` 标记的命令/查询自动生成为 Microsoft Agent Framework 工具，复用与 HTTP 相同的管道
- **常用组件**：HybridCache 混合缓存（L1 内存 + L2 Redis）、RedLock 分布式锁、阿里云 OSS 文件提供器、ClickHouse 批量写入

## 快速开始

安装所需的包（如 `Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection` 与 `Cnblogs.Architecture.Ddd.Cqrs.AspNetCore`），然后：

```csharp
builder.Services.AddCqrs(typeof(CreateArticleCommand).Assembly)
    .AddHybridQueryCache()
    .AddEnrichers();

var apis = app.NewVersionedApi();
var v1 = apis.MapGroup("/api/v{version:apiVersion}").HasApiVersion(1);

v1.MapQuery<ListArticlesQuery>("articles");                    // GET /api/v1/articles，分页参数自动绑定
v1.MapCommand("articles", ([FromBody] CreateArticlePayload p)  // POST /api/v1/articles，动词由命令名推断
    => new CreateArticleCommand(p.Title));
```

## 文档

- [框架指南](docs/framework.md)：自底向上介绍各层包提供的能力与推荐用法，含包总览
- [How-to 系列](docs/how-to/)：按任务查找具体做法——
  [定义聚合根与领域事件](docs/how-to/define-aggregate.md)、
  [实现命令](docs/how-to/implement-command.md)、
  [实现查询](docs/how-to/implement-query.md)、
  [暴露 HTTP API](docs/how-to/expose-api.md)、
  [生成与使用 ServiceAgent](docs/how-to/setup-service-agent.md)、
  [查询缓存](docs/how-to/cache-queries.md)~~~~、
  [用 Enricher 聚合外部数据](docs/how-to/enrich-query-results.md)、
  [发布与订阅集成事件](docs/how-to/publish-integration-events.md)、

## License

[MIT](LICENSE)
