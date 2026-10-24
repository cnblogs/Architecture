# How-to：暴露 HTTP API

**场景**：把命令与查询暴露为 HTTP 端点——选路由、选动词、配置版本与错误响应。

## 端点骨架

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseCnblogsSerilog();

builder.Services.AddControllers().AddCqrsModelBinderProvider();
builder.Services.AddCnblogsApiVersioning();
builder.Services.AddCqrs(typeof(GetInvoiceQuery).Assembly, typeof(CreateInvoiceCommand).Assembly)
    .AddEventBus(o => o.UseDapr(typeof(InvoiceIssuedIntegrationEvent).Assembly))
    .AddDefaultDateTimeAndRandomProvider();

var app = builder.Build();
app.UseForwardedHeaders();
app.UseAuthorization();

var apis = app.NewVersionedApi();
var v1 = apis.MapGroup("/api/v{version:apiVersion}").HasApiVersion(1);
// ... MapQuery / MapCommand ...
```

优先使用 Minimal API；控制器风格仅用于内网管理后台等既有 MVC 场景（见文末）。

## 查询端点：MapQuery

```csharp
v1.MapQuery<ListInvoicesQuery>("invoices");                                // ?pageIndex=&pageSize=&orderByString=
v1.MapQuery<GetInvoiceQuery>("invoices/{invoiceId:long}");                  // 找不到自动 404
v1.MapQuery<BatchGetInvoicesQuery>("invoices:batchGet");                   // ?ids=1&ids=2
v1.MapQuery<ListNotesQuery>("corporations/-/notes:batchGet");              // - 占位：父 Id 不适用
v1.MapQuery<GetOrderQuery>("corporations/{corpId:int}/orders/{id:long}", MapNullableRouteParameter.Enable);
```

- 查询参数（含 `PagingParams`、`OrderByString`）自动从路由/查询字符串绑定。
- `MapNullableRouteParameter.Enable` 为可空路由参数生成 `-` 占位变体（`corporations/-/orders/1` 与 `corporations/5/orders/1` 都能命中）。
- `enableHead: true` 生成配套 HEAD 端点（存在性检查）。

## 命令端点：MapCommand 家族

```csharp
// 动词由命令名推断：CreateInvoiceCommand → POST
v1.MapCommand("invoices", ([FromBody] CreateInvoicePayload payload)
    => new CreateInvoiceCommand(payload.Title /*, ... */));

// 显式动词
v1.MapPutCommand("invoices/{invoiceId:long}:issue",
    (long invoiceId, [FromBody] IssueInvoicePayload payload)
    => new IssueInvoiceCommand(invoiceId, payload.InvoiceNumber /*, ... */));
v1.MapPutCommand("invoices/{invoiceId:long}/remark",
    (long invoiceId, [FromBody] UpdateRemarkPayload payload) => new UpdateInvoiceCommand(...));
v1.MapDeleteCommand<RemoveNoteCommand>("corporations/{corpId:int}/notes/{id:int}");
```

- 带 `[FromBody]` 请求体的命令用 **Delegate 形式**：从 Payload record 构造命令。Payload 放在 `MyApp.Api.Payloads` 项目，它是 ServiceAgent 契约的一部分。
- 参数来自路由/查询的命令（DELETE、简单 PUT）用**泛型形式** `MapDeleteCommand<TCommand>(route)`，命令按 `[AsParameters]` 绑定。

## 路由风格（Google Cloud AIP）

| 规则 | 示例 |
| --- | --- |
| 资源复数、小写连字符 | `/entity-groups/{groupId}/entities` |
| 层级表达从属关系 | `/corporations/{corpId}/licenses/{licenseId}` |
| Id 不单个/不适用时用 `-` 占位 | `/corporations/-/notes:batchGet` |
| 自定义动词 camelCase 后缀，选最接近的 HTTP 动词 | `GET :batchGet`、`PUT {id}:cancel`、`POST :batchUpdate` |
| 能用子资源就不用自定义动词 | `PUT invoices/{id}/remark` 优于 `PUT invoices/{id}:updateRemark` |
| 名词优先 | `/entities` 而非 `/get-entities` |

自定义动词的动词选择：幂等用 `PUT`，非幂等用 `POST`，只读用 `GET`。

## 错误响应

默认行为：校验失败/业务失败 400 纯文本；锁冲突 429；成功带返回体 200，无返回体 204。

```csharp
services.AddCqrs(/*...*/)
    .UseProblemDetails()                    // 400 改为 ProblemDetails（RFC 7807）
    .AddLongToStringJsonConverter();        // long 序列化为字符串，避免前端精度丢失
```

需要完整错误结构（含 `ErrorCode`）的客户端，发送 `Accept: application/cqrs` 头即可拿到 `CommandResponse` JSON 信封——ServiceAgent 默认就这么做。更复杂的定制用 `UseCustomCommandErrorResponseMapper(...)`。

## 编排：端点/Controller 是合法位置

一个 HTTP 请求需要先查询再发多个命令时，在端点 delegate（或 Controller 动作）里编排，不要在 Handler 里嵌套 `mediator.Send`：

```csharp
v1.MapPost("orders/{orderId:long}:resync",
    async (long orderId, IMediator mediator, CancellationToken ct) =>
    {
        var order = await mediator.Send(new GetOrderQuery(orderId), ct);
        if (order is null)
            return Results.NotFound();
        return await mediator.Send(new ResyncOrderCommand(orderId), ct) is { IsSuccess: true }
            ? Results.Ok()
            : Results.BadRequest();
    });
```

## 控制器风格 - 需要复杂编排时可用

```csharp
[ApiController]
[Route("/api/v{version:apiVersion}/invoices")]
public class InvoiceController(IMediator mediator) : ApiControllerBase
{
    [HttpPut("{invoiceId:long}/status")]
    public async Task<IActionResult> UpdateStatus(long invoiceId, UpdateStatusPayload payload)
        => HandleCommandResponse(await mediator.Send(new UpdateInvoiceStatusCommand(invoiceId, payload.Status)));
}
```

`ApiControllerBase.HandleCommandResponse` 提供与 Minimal API 一致的 200/204/400 语义；`AddControllers().AddCqrsModelBinderProvider()` 让控制器动作也能绑定 `PagingParams`。
