# How-to：实现查询

**场景**：为资源提供读接口——单项、分页列表、批量获取；涉及分页、排序、DTO 映射。

## 三个约定

1. **一个资源三种查询**：`GetEntityQuery`（按 Id 取单个）、`ListEntitiesQuery`（分页列表）、`BatchGetEntitiesQuery`（按 Id 批量取）。
2. **参数化优先**：新增需求先考虑给现有查询加可选参数（`int?`、数组参数），而不是新建一个几乎相同的查询。
3. **查询不报错**：查不到返回 `null`（HTTP 404）或空集合，方便缓存；明显不合法的参数（如空 Id）提前返回空值，避免无谓的数据库查询。

## GetEntityQuery：单项查询

```csharp
public record GetInvoiceQuery(long InvoiceId) : IQuery<InvoiceDto?>;

public class GetInvoiceQueryHandler(IInvoiceRepository repository)
    : IQueryHandler<GetInvoiceQuery, InvoiceDto?>
{
    public async Task<InvoiceDto?> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        return await repository.NoTrackingQueryable
            .Where(x => x.Id == request.InvoiceId)
            .ProjectToType<InvoiceDto>()          // Mapster 服务端投影
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

- 查询一律走 `NoTrackingQueryable`，在 Handler 里自由组装条件（仓储不提供查询方法）。
- `ProjectToType<TDto>` 在数据库端完成投影；导航属性的展开交给 Mapster 配置（见下文），不需要显式 `Include`。

## ListEntitiesQuery：分页列表

```csharp
public record ListCorporationOrdersQuery(
    int? CorpId = null,
    bool? IsGift = null,
    CorporationOrderPayStatus[]? PayStatuses = null,
    string? Search = null,
    PagingParams? PagingParams = null,
    string? OrderByString = null) : IPageableModelQuery<CorporationOrderDto>;

public class ListCorporationOrdersQueryHandler(
    ICorporationOrderRepository orderRepository,
    ICorporationContractRepository contractRepository)
    : EfPageableModelQueryHandler<ListCorporationOrdersQuery, CorporationOrder, CorporationOrderDto>
{
    protected override IQueryable<CorporationOrder> DefaultOrderBy(
        ListCorporationOrdersQuery query,
        IQueryable<CorporationOrder> queryable)
        => queryable.OrderByDescending(x => x.Id);

    protected override IQueryable<CorporationOrder> Filter(ListCorporationOrdersQuery query)
    {
        var queryable = orderRepository.NoTrackingQueryable;
        if (query.CorpId is { } corpId)
            queryable = queryable.Where(x => x.CorpId == corpId);
        if (query.PayStatuses?.Length > 0)
            queryable = queryable.Where(x => query.PayStatuses.Contains(x.PayStatus));
        if (string.IsNullOrWhiteSpace(query.Search) == false)
        {
            var keyword = query.Search.Trim();
            queryable = long.TryParse(keyword, out var id)
                ? queryable.Where(x => x.Id == id)
                : queryable.Where(x => x.Corporation.Name.Contains(keyword));
        }
        return queryable;
    }
}
```

基类 `EfPageableQueryHandler<TQuery, TEntity, TView>`（DTO 实现 `IModel` 时用 `EfPageableModelQueryHandler`）负责解析 `OrderByString`、计数、分页与 Mapster 投影，只需要实现 `Filter` 和 `DefaultOrderBy`。

`PagingParams` 由框架从路由或查询字符串自动绑定（`?pageIndex=1&pageSize=20`）。两个内建行为：

- **`pageSize=0` 只取总数**：返回的 `PagedList` 只填 `TotalCount`，`Items` 为空。统计数量不需要单独的 Count 查询。
- **负数 `pageIndex` 倒序取页**：按默认排序的反方向分页（用于"最近 N 条"类需求，可覆写 `DefaultReverseOrderBy`）。

## BatchGetEntitiesQuery：批量查询

```csharp
public record BatchGetInvoicesQuery(long[] Ids) : IListQuery<List<InvoiceDto>>;

public class BatchGetInvoicesQueryHandler(IInvoiceRepository repository)
    : IListQueryHandler<BatchGetInvoicesQuery, List<InvoiceDto>>
{
    public async Task<List<InvoiceDto>> Handle(BatchGetInvoicesQuery request, CancellationToken ct)
        => await repository.NoTrackingQueryable
            .Where(x => request.Ids.Contains(x.Id))
            .ProjectToType<InvoiceDto>()
            .ToListAsync(ct);
}
```

HTTP 侧映射为 `GET /invoices:batchGet?ids=1&ids=2`，ServiceAgent 生成 `BatchGetInvoicesAsync(long[] ids)`。

## 不分页的小列表：IListQuery

预期很小、不需要分页的一对多查询（省市列表、地址列表）直接用 `IListQuery<TList>`，并显式指明集合类型，减少调用方判空：

```csharp
public record ListProvincesQuery() : IListQuery<List<ProvinceDto>>;
public record GetUserAddressesQuery(Guid UserId) : IListQuery<Dictionary<string, AddressDto>>;
```

## 排序：注册白名单

`OrderByString`（`-dateAdded` 降序、`id` 升序、逗号分隔多字段）只能使用注册过的字段。集中在一个静态方法里注册，启动时调用：

```csharp
public static class ServiceInjector
{
    public static IServiceCollection ConfigureOrderByString(this IServiceCollection services)
    {
        OrderBySegmentConfig.RegisterSortableProperty<InvoiceRecord, DateTimeOffset>(
            nameof(InvoiceRecord.DateAdded), o => o.DateAdded);
        OrderBySegmentConfig.RegisterSortableProperty<CorporationOrder, int>(
            "payStatus", x => x.PayStatus);
        return services;
    }
}
```

## 映射：Mapster

同名字段自动映射，导航属性由 Mapster 自动展开投影，**不要**为此写 `Include`。需要定制时集中配置，同样以静态方法在启动时调用：

```csharp
public static IServiceCollection ConfigureMapster(this IServiceCollection services)
{
    TypeAdapterConfig<Corporation, CorporationDto>.NewConfig()
        .Map(dest => dest.CorpName, src => src.Corporation.Name)
        .Map(dest => dest.SignerCount, src => src.Signers.Count);
    TypeAdapterConfig<BlogPost, BlogPostDto>.NewConfig()
        .Map(dest => dest.Tags, src => src.Tags.Where(t => t.IsActive).Select(t => t.Name));
    return services;
}
```

注意：软删除过滤由 EF QueryFilter 全局处理，查询与 Mapster 配置里都不要再手写 `x.Deleted == false`。

单元测试里调用 `Adapt`/`ProjectToType` 前，需要用 `[ModuleInitializer]` 调一次上面的配置方法，保证与生产行为一致。

## 查询结果需要外部数据？

不要在查询 Handler 里调外部服务/其他数据库——定义 Enricher，见 [enrich-query-results](enrich-query-results.md)。需要缓存见 [cache-queries](cache-queries.md)。
