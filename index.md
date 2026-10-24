---
_layout: landing
---

# Cnblogs.Architecture

一系列用于快速构建 DDD + CQRS 微服务的 NuGet 包，基于 .NET 10，发布在 [nuget.org](https://www.nuget.org/packages?q=Cnblogs.Architecture)。框架把实体与聚合、领域事件、命令/查询管道、HTTP 端点映射、服务间强类型调用等基础设施沉淀为可复用的组件，让你把精力集中在领域划分和业务逻辑上。

## [框架指南](docs/framework.md)

自底向上介绍各层包提供的能力与推荐用法：Domain、Infrastructure、Application（CQRS）、DI 组装、API 层、ServiceAgent、集成事件与 AI 工具集成，附完整包总览。

## [How-to 系列](docs/how-to/define-aggregate.md)

按任务查找具体做法：定义聚合根与领域事件、实现命令与查询、暴露 HTTP API、生成与使用 ServiceAgent、查询缓存、用 Enricher 聚合外部数据、发布与订阅集成事件。

## [快速开始](docs/framework.md#di-组装)

安装 NuGet 包后 `AddCqrs()` 一行注册命令/查询处理器，用 `MapQuery`/`MapCommand` 把它们映射为 Google Cloud AIP 风格的 HTTP 端点，校验、缓存、分布式锁、Enricher 等管道行为按需叠加。

## [GitHub](https://github.com/cnblogs/Architecture)

源码、Release 与 Issue。
