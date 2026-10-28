# How-to：定义聚合根与领域事件

**场景**：新建（或重构）一个业务模型，需要决定实体怎么放、聚合边界在哪、状态变更怎么落地。

## 步骤

### 1. 聚合根继承 Entity<TKey> 并实现 IAggregateRoot

```csharp
public class Corporation : Entity<int>, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public List<CorporationMember> Members { get; private set; } = [];
    public List<CorporationLicense> Licenses { get; private set; } = [];

    private Corporation() { }   // EF 需要

    private Corporation(string name) { Name = name; }

    public static Corporation Create(string name)
    {
        var corporation = new Corporation(name);
        corporation.AddDomainEvent(new CorporationCreatedDomainEvent(corporation)); // Id 尚未生成，用延迟事件
        return corporation;
    }
}
```

要点：

- 主键通常用 `int` 自增；应用侧生成单号（如 `20230615000001`）或数据量大且主键无业务含义时才用 `long`（配 `IIdProvider`）或 `Guid`。
- 属性 `private set`，对外只暴露行为方法——这是聚合不变量的第一道防线。
- 实体还没有 Id 就要发事件时，用延迟生成器 `AddDomainEvent(id => new XxxCreatedDomainEvent(id))`，事件在派发时才用实际 Id 构造。
- 子实体（如 `CorporationMember : Entity<int>`）不需要 `IAggregateRoot`，通过聚合根的导航集合持有。
- 软删除调用仓储的 `DeleteAsync` / `DeleteRangeAsync` 即可，默认把 `Deleted` 置为 `true`（需真正删除时传 `softDelete: false`）；聚合内部移除子实体时也可以直接置 `Deleted = true`。查询过滤交给 EF QueryFilter（见第 5 步）。

### 2. 把状态变更写成聚合方法，并在方法内记录领域事件

```csharp
// 状态机校验 + 修改 + 事件
public void Issue(string invoiceNumber, string? invoiceCode, DateTimeOffset issuedDate, Guid issuerId)
{
    if (Status != InvoiceStatus.Requested)
        throw new InvalidOperationException($"只有待开票状态的发票才能登记开票，当前状态：{Status}");
    Status = InvoiceStatus.Issued;
    InvoiceNumber = invoiceNumber;
    // ...
    AddDomainEvent(new InvoiceIssuedDomainEvent(Id, App, UserId, BuyerId, GetOrderIds()));
}

// 软删除子实体
public void RemoveFile(long id)
{
    var file = Files.FirstOrDefault(x => x.Id == id);
    if (file is null)
        return;
    file.Deleted = true;
}
```

子实体的增删改都经由聚合根方法完成（`AddChild`、`RevokeLicense`、`RemoveFile`...），跨聚合的联动通过领域事件通知，不在聚合内部直接引用其他聚合。

### 3. 台账/余额类模型：所有变更走方法，每次变更留下流水

`CorporationPostQuotaAccount` 是典型模型：`Balance` 只能通过 `Recharge/Allocate/Recall/Deduct/TransferOut/TransferIn` 修改，每个方法向 `Transactions` 集合追加一条流水并更新 `Allocations`。审计诉求（谁在何时改了多少）天然满足，不需要额外的日志表逻辑侵入业务代码。

### 4. 仓储接口放 Domain，实现放 Infrastructure

```csharp
// Domain
public interface ICorporationRepository : INavigationRepository<Corporation, int> { }

// Infrastructure
public class CorporationRepository(IMediator mediator, CorpDbContext context)
    : BaseRepository<CorpDbContext, Corporation, int>(mediator, context), ICorporationRepository;
```

`INavigationRepository` 提供 `GetAsync(key, includes)`，是加载聚合导航数据的入口。仓储实现不写查询逻辑——查询在 Handler 里用 `NoTrackingQueryable` 组装。

### 5. EF 配置：QueryFilter 以及索引

```csharp
public class CorporationConfiguration : IEntityTypeConfiguration<Corporation>
{
    public void Configure(EntityTypeBuilder<Corporation> builder)
    {
        builder.HasQueryFilter(x => x.Deleted == false);   // 软删除过滤只在这里写一次
        builder.HasIndex(x => x.ShortName)
            .IsUnique()
            .HasFilter("[Deleted] = 0 AND [ShortName] IS NOT NULL");  // 部分索引排除软删除
    }
}
```

使用应用侧生成的 `long` Id 时记得 `builder.Property(x => x.Id).ValueGeneratedNever()`，Id 由 Handler 里 `IIdProvider.NextReadable(...)` 生成。

## 加载与保存：一次 GetAsync，一次 UpdateAsync

聚合内部的多处修改放进同一个事务的推荐路径：

```csharp
var corporation = await corporationRepository.GetAsync(
    request.CorpId,
    x => x.Licenses.Where(l => l.Id == request.LicenseId));   // 过滤导航属性，只加载需要的子项
corporation.RevokeLicense(request.LicenseId, request.OperatorId);
await corporationRepository.UpdateAsync(corporation);          // SaveChanges + 派发领域事件
```

- 大多数时候不需要 Include 整个导航属性，用 `Where` 过滤。
- 需要多层路径时可用字符串：`GetAsync(id, ["Children.GrandChildren"])`——但先反问自己是否真的需要：多层多对多级联往往说明子聚合该独立出来，或聚合边界画错了。
- Mapster 也能在投影时自动处理导航属性（见 [implement-query](implement-query.md)），查询侧不要为了 DTO 手工 Include。

## 单元测试直接测聚合方法

聚合不依赖基础设施，测试直接 new：

```csharp
[Fact]
public void Issue_StatusIsRequested_ExpectStatusChanged()
{
    // Arrange
    var invoice = new InvoiceRecord(/* ... 待开票状态 ... */);

    // Act
    invoice.Issue("INV-001", null, DateTimeOffset.Now, Guid.NewGuid());

    // Assert
    Assert.Equal(InvoiceStatus.Issued, invoice.Status);
}
```

## 下一步

- 状态变更如何暴露为命令：[implement-command](implement-command.md)
- 领域事件如何触发集成事件/缓存失效：[publish-integration-events](publish-integration-events.md)、[cache-queries](cache-queries.md)
