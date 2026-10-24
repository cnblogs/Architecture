# How-to：生成与使用 ServiceAgent

**场景**：让其他服务强类型地调用本服务 API；或在本服务里消费上游服务的 ServiceAgent。

## 前提：API 用 MapQuery / MapCommand 注册端点

生成器从真实端点导出契约，因此 API 必须用 `MapQuery<T>` / `MapCommand<T>`（或 Delegate 形式）注册，路由里不要夹带手工 `MapGet`。`WithServiceAgentGroup<TBuilder>(name, errorType)` 可把端点分组，生成器按组生成多个 Service。

## 服务端：生成

1. 给 API 项目添加设计包（仅生成时生效，运行时是 no-op）：

```bash
dotnet add ./src/MyApp.Api package Cnblogs.Architecture.ServiceAgent.Design
```

2. 安装全局工具并生成：

```bash
dotnet tool install -g dotnet-cnb

dotnet cnb serviceagent generate \
  --api-project ./src/MyApp.Api \
  --output ./src/MyApp.ServiceAgent \
  --namespace MyApp.ServiceAgent \
  --base-url http://myapp-api \
  --clean
```

| 参数 | 说明 |
| --- | --- |
| `--api-project` | API 项目路径（csproj 或目录） |
| `--output` | 生成目录 |
| `--namespace` | 生成代码的命名空间 |
| `--base-url` | 写入 `AddMyAppService()` 默认注册地址（如 k8s/Dapr 服务名 `http://myapp-api`） |
| `--api-version N` | 只生成第 N 版端点（默认每个版本一组，如 `IMyAppV1Service`/`IMyAppV2Service`） |
| `--app-name X` | 把所有分组合并为一个 `IXService` |
| `--clean` | 清理上次生成、标记为 `dotnet-cnb` 生成的文件 |

生成原理与 `dotnet ef` 相同：Design 包通过 HostingStartup 在临时启动时把端点导出为 manifest，工具再把 manifest 变成代码。API 重启不受影响。

## 产物

```
MyApp.ServiceAgent/
  IMyAppService.cs          # 每个端点一个方法，签名使用 Payload/Dto/枚举
  MyAppService.cs            # 继承 CqrsServiceAgent<MyAppError>，路由与 Minimal API 一一对应
  ServiceAgentExtensions.cs  # AddMyAppService(baseUri?) 注册扩展
```

分页端点生成 `Task<PagedList<InvoiceDto>> ListInvoicesAsync(int? pageIndex = null, int? pageSize = null, string? orderByString = null)`；命令生成 `Task<CommandResponse<TView?, InvoiceError>>`；HEAD 端点生成 `HasXxxAsync()`；`-` 占位路由生成可空参数（`corpId?.ToString() ?? "-"`）。

注意：`ValidateOnly` 没有特殊处理，它作为命令的普通属性随 Payload 传递。

## 打包发布

ServiceAgent 项目的 csproj 用 `PrivateAssets="all"` 的 ProjectReference 引用 Payloads/Dto/IntegrationEvents/ValueObjects，配合 `CopyProjectReferencesToPackage` Target 把契约程序集内嵌进 nupkg，消费方不需要再引用这些项目。用 `pack.sh`（版本号取 git tag）推送到私有 NuGet 源。

## 消费端：注册与调用

```csharp
// Program.cs / Bootstrapper
services.AddServiceAgent<IMyAppService, MyAppService>("http://myapp-api");
```

`AddServiceAgent` 注册 Typed HttpClient：自动附加 `Accept: application/cqrs`、`X-Cqrs-Version: 2`（获取完整 `CommandResponse` 信封），配置标准 resilience（GET 等安全方法重试最多 3 次，POST 不自动重试）。

```csharp
public class RegisterInvoiceFileCommandHandler(IUploadService uploadService)
    : ICommandHandler<RegisterInvoiceFileCommand, InvoiceError>
{
    public async Task<CommandResponse<InvoiceError>> Handle(/*...*/)
    {
        var file = await uploadService.GetUploadFileAsync(request.FileId);
        if (file is null)
            return CommandResponse<InvoiceError>.Fail(InvoiceError.FileNotFound);
        // ...
    }
}
```

命令结果按 `CommandResponse` 判断：`response.IsSuccess()` / `response.ErrorCode`，与本地命令的处理方式一致。

## 已知限制

- 路由 token 没有对应命令参数时该端点被跳过；重复签名保留第一个。
- DELETE 带请求体不支持。
- 生成的 Payload POCO 不携带 `[JsonPropertyName]` 等序列化特性。
- 手写的 `MapGet`/`MapPost` 端点没有 CQRS 描述符，不参与生成。
