# How-to：实现一个命令

**场景**：把一个写操作（创建、状态变更、删除...）实现为 CQRS 命令，包含校验、错误返回、防重与干跑。

## 1. 定义命令与错误码

```csharp
// 错误码集中在一个 Enumeration 子类
public class InvoiceError : Enumeration
{
    public static readonly InvoiceError ItemAlreadyInvoiced = new(1, "商品已在其他发票中");
    public static readonly InvoiceError InvalidStatus = new(2, "发票状态不正确");

    [method: JsonConstructor]
    private InvoiceError(int id, string name) : base(id, name) { }
}

// 无返回体
public record CancelInvoiceCommand(long InvoiceId, Guid OperatorId, bool ValidateOnly = false)
    : ICommand<InvoiceError>;

// 有返回体
public record CreateInvoiceCommand(/* ... */) : ICommand<InvoiceDto, InvoiceError>, IValidatable;
```

约定：

- `ValidateOnly` 参数总是放在最后，默认 `false`。
- 命令名前缀决定 Minimal API 的 HTTP 动词（`Create/Add/New` → POST、`Update/Modify/Replace/Alter` → PUT、`Delete/Remove/Clean/Clear/Purge` → DELETE），见 [expose-api](expose-api.md)。

## 2. 静态格式校验放命令上（IValidatable）

不需要查库的校验实现 `IValidatable`，框架管道自动执行并返回 400，Handler 不会运行：

```csharp
public record CreateCorporationContractCommand(
    int CorpId,
    string Title,
    string Content,
    bool ValidateOnly = false) : ICommand<CorporationContractDto, CorpError>, IValidatable
{
    public void Validate(ValidationErrors errors)
    {
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new ValidationError("合同标题不能为空", nameof(Title)));
        if (Title.Length > 200)
            errors.Add(new ValidationError("合同标题不能超过 200 个字符", nameof(Title)));
    }
}
```

查询不做 `IValidatable` 校验——查询是只读操作，明显不合法的参数在 Handler 里提前返回空值即可。

## 3. Handler：逻辑校验 → ValidateOnly 早返回 → 执行

```csharp
public class CreateInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IIdProvider idProvider) : ICommandHandler<CreateInvoiceCommand, InvoiceDto, InvoiceError>
{
    public async Task<CommandResponse<InvoiceDto, InvoiceError>> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 逻辑校验：查库判断业务规则（用 NoTrackingQueryable）
        var invoicedKeys = await invoiceRepository.NoTrackingQueryable
            .Where(/* 已开票的 (OrderId, OutProductId) 集合 */)
            .Select(x => new { x.OrderId, x.OutProductId })
            .ToListAsync(cancellationToken);
        var conflicts = items.Where(x => invoicedKeys.Contains((x.OrderId, x.OutProductId))).ToList();
        if (conflicts.Count > 0)
            return CommandResponse<InvoiceDto, InvoiceError>.Fail(InvoiceError.ItemAlreadyInvoiced);

        // 干跑：所有校验已通过，直接返回成功，不落库、不带返回体
        if (request.ValidateOnly)
            return CommandResponse<InvoiceDto, InvoiceError>.Success();

        // 执行
        var invoiceId = idProvider.NextReadable();   // 应用侧生成的 long 主键
        var invoice = InvoiceRecord.Create(/* ... */);
        await invoiceRepository.AddAsync(invoice);

        return CommandResponse<InvoiceDto, InvoiceError>.Success(invoice.Adapt<InvoiceDto>());
    }
}
```

要点：

- `ValidateOnly` 的返回在**所有校验之后、任何写入之前**；干跑时即使返回类型带 `TView` 也只返回 `Success()`，不带返回体。
- 逻辑校验失败返回 `Fail(errorCode)`，错误信息来自错误码 `Name`，不要抛异常表达业务失败。
- 修改已有聚合：`GetAsync(id, 导航)` → 调聚合方法 → `UpdateAsync`（见 [define-aggregate](define-aggregate.md)）。

## 4. 需要防并发时加分布式锁

命令实现 `ILockableRequest`，配合 `AddRedLockDistributionLock(configuration)`：

```csharp
public record CreateInvoiceCommand(/* ... */)
    : ICommand<InvoiceDto, InvoiceError>, IValidatable, ILockableRequest
{
    // 以业务去重维度构造锁键：同一批订单同时只能有一张发票在开
    public string GetLockKey() => $"invoice:create:{string.Join('-', OrderIds.Order())}";
}
```

抢锁失败时管道直接返回 `IsConcurrentError = true`，HTTP 侧映射为 429，Handler 不会执行。锁是性能/竞态保护，业务正确性仍以 Handler 内的冲突校验为准（双保险）。

## 5. 禁止在 Handler 里嵌套调用其他命令/查询

不要在 Application 层的 Handler 里 `mediator.Send` 另一个命令或查询——嵌套调用会让逻辑分散在多层调用链里，难测试难追踪。可选的编排位置：

1. **Controller / Minimal API 端点**里编排多个命令与查询（推荐，调用链一目了然）；
2. 在 Web/Api 项目**新增**一个编排用的命令/查询，其 Handler 调 Application 层的多个命令/查询；
3. 大段重复代码（>50 行）才考虑提取 Service/扩展方法，不要封装简单的查询条件。

## 6. 单元测试（AAA + InMemory + NSubstitute）

```csharp
[Fact]
public async Task HandleAsync_OrderAlreadyInvoiced_ExpectFail()
{
    // Arrange
    var context = new InvoiceDbContext(new DbContextOptionsBuilder<InvoiceDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    await context.Invoices.AddAsync(/* 已开票同订单的记录 */);
    await context.SaveChangesAsync();
    var repository = new InvoiceRepository(Substitute.For<IMediator>(), context);
    var handler = new CreateInvoiceCommandHandler(repository, Substitute.For<IIdProvider>());

    // Act
    var response = await handler.Handle(new CreateInvoiceCommand(/* ... */), CancellationToken.None);

    // Assert
    Assert.False(response.IsSuccess());
    Assert.Equal(InvoiceError.ItemAlreadyInvoiced, response.ErrorCode);
}
```

- 用 EF InMemory + `new` 出来的仓储，Mock `IMediator` 与外部 ServiceAgent；一般不需要 Mock 仓储本身。
- 静态校验可以单独测命令的 `Validate` 方法。
