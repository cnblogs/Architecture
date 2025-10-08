using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.TestShared;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Handlers;

public class PageableQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoPaging_AllItemsAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        var response = await handler.Handle(new TestPageableQuery(null, null, null), CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, response.TotalCount);
        Assert.Equal(1, response.PageIndex);
        Assert.Equal(posts.Count, response.PageSize);
        Assert.Equal(posts.Count, response.Items.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_ZeroOrNegativePageSize_EmptyListWithTotalCountAsync(int pageSize)
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = 1;
        var response = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), null),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, response.TotalCount);
        Assert.Equal(pageIndex, response.PageIndex);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Empty(response.Items);
    }

    [Fact]
    public async Task Handle_Paging_PagedItemsWithTotalCountAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        var newestPostId = posts.OrderByDescending(x => x.DateAdded).ThenByDescending(x => x.DateUpdated).First().Id;
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = 1;
        const int pageSize = 3;
        var response = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), null),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, response.TotalCount);
        Assert.Equal(pageIndex, response.PageIndex);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(pageSize, response.Items.Count);
        Assert.Equal(newestPostId, response.Items.First().Id);
    }

    [Fact]
    public async Task Handle_NegativeIndexing_PagedItemsWithTotalCountAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        var oldestPostId = posts.OrderBy(x => x.DateAdded).ThenBy(x => x.DateUpdated).First().Id;
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = -1;
        const int pageSize = 3;
        var response = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), null),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, response.TotalCount);
        Assert.Equal(pageIndex, response.PageIndex);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(pageSize, response.Items.Count);
        Assert.Equal(oldestPostId, response.Items.First().Id);
    }

    [Fact]
    public async Task Handle_OrderByString_OverrideDefaultOrderByAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        OrderBySegmentConfig<FakePost>.RegisterSortableProperty("id", x => x.Id);
        var minId = posts.Min(x => x.Id);
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = 1;
        const int pageSize = 3;
        var response = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), "id"),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, response.TotalCount);
        Assert.Equal(pageIndex, response.PageIndex);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(pageSize, response.Items.Count);
        Assert.Equal(minId, response.Items.First().Id);
    }

    [Fact]
    public async Task Handle_OrderByStringWithNegativeIndexing_OverrideDefaultOrderByAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        OrderBySegmentConfig<FakePost>.RegisterSortableProperty("id", x => x.Id);
        var maxId = posts.Max(x => x.Id);
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = -1;
        const int pageSize = 3;
        var negativeIndexing = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), "id"),
            CancellationToken.None);
        var descendingOrderBy = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(-pageIndex, pageSize), "-id"),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, negativeIndexing.TotalCount);
        Assert.Equal(pageIndex, negativeIndexing.PageIndex);
        Assert.Equal(pageSize, negativeIndexing.PageSize);
        Assert.Equal(pageSize, negativeIndexing.Items.Count);
        Assert.Equal(maxId, negativeIndexing.Items.First().Id);
        Assert.Equivalent(negativeIndexing.Items, descendingOrderBy.Items);
    }

    [Fact]
    public async Task Handle_MultipleOrderBySegment_OverrideDefaultOrderByAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        OrderBySegmentConfig<FakePost>.RegisterSortableProperty("dateAdded", x => x.DateAdded);
        OrderBySegmentConfig<FakePost>.RegisterSortableProperty("dateUpdated", x => x.DateUpdated);
        var newestPostId = posts.OrderByDescending(x => x.DateAdded).ThenByDescending(x => x.DateUpdated).First().Id;
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = 1;
        const int pageSize = 3;
        var withOrderByString = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), "-dateAdded,-dateUpdated"),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, withOrderByString.TotalCount);
        Assert.Equal(pageIndex, withOrderByString.PageIndex);
        Assert.Equal(pageSize, withOrderByString.PageSize);
        Assert.Equal(pageSize, withOrderByString.Items.Count);
        Assert.Equal(newestPostId, withOrderByString.Items.First().Id);
    }

    [Fact]
    public async Task Handle_MultipleOrderBySegmentWithNegativeIndexing_EquivlentToNelegateOrderByStringAsync()
    {
        // Arrange
        var posts = new EntityGenerator<FakePost>(new FakePost())
            .VaryByDateTimeDay(x => x.DateAdded, 5)
            .VaryByDateTimeDay(x => x.DateUpdated, 2)
            .FillWithInt(x => x.Id, 1, 100)
            .Generate();
        OrderBySegmentConfig<FakePost>.RegisterSortableProperty("dateAdded", x => x.DateAdded);
        OrderBySegmentConfig<FakePost>.RegisterSortableProperty("dateUpdated", x => x.DateUpdated);
        var oldestPostId = posts.OrderBy(x => x.DateAdded).ThenBy(x => x.DateUpdated).First().Id;
        var dbContext = await GetDbContextAsync(posts);
        var handler = new TestPageableQueryHandler(dbContext);

        // Act
        const int pageIndex = -1;
        const int pageSize = 3;
        var negativeIndexing = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(pageIndex, pageSize), "-dateAdded,-dateUpdated"),
            CancellationToken.None);
        var negativeOrdering = await handler.Handle(
            new TestPageableQuery(null, new PagingParams(-pageIndex, pageSize), "dateAdded,dateUpdated"),
            CancellationToken.None);

        // Assert
        Assert.Equal(posts.Count, negativeIndexing.TotalCount);
        Assert.Equal(pageIndex, negativeIndexing.PageIndex);
        Assert.Equal(pageSize, negativeIndexing.PageSize);
        Assert.Equal(pageSize, negativeIndexing.Items.Count);
        Assert.Equal(oldestPostId, negativeIndexing.Items.First().Id);
        Assert.Equivalent(negativeIndexing.Items, negativeOrdering.Items);
    }

    private static async Task<DbContext> GetDbContextAsync(ICollection<FakePost> entities)
    {
        var options = new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options;
        var context = new FakeDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await context.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        return context;
    }
}
