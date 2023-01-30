# Cnblogs.Architecture

`Cnblogs.Architecture` 是一系列用于快速构建 DDD+CQRS 微服务的 Nuget 包，您不再需要自己定义 DDD 和 CQRS 相关的基础设施，只需要关注领域划分和业务逻辑，减少思维负担并加快开发速度。

## DDD 划分及 CQRS 实现

设计上将项目分为四层，从内到外分别是 Domain, Infrastructure, Application 和 API/Web/ServiceAgent，外层依赖内层，内层不会依赖外层。

各层包含的内容如下：

- Domain：实体、领域事件及 Repository 接口
- Infrastructure：Repository 实现以及各类基础设施 SDK 需要的类（如数据库 DbContext，缓存 SDK，三方 API SDK 等）
- Application：业务逻辑（命令和查询），领域事件处理器，订阅的集成事件处理器，自身的集成事件定义
- API/Web/ServiceAgent：提供 Web 服务和身份验证，根据路径调用 Application 层的对应命令和查询，尽量不包含业务逻辑