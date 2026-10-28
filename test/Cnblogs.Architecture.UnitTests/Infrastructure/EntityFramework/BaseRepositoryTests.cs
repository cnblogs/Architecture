using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.TestShared;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.EntityFramework;

public class BaseRepositoryTests
{
    [Fact]
    public async Task GetEntityAsync_Include_GetEntityAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost())
                    .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var got = await repository.GetAsync(entity.Id, e => e.Posts);

        // Assert
        Assert.NotNull(got);
        Assert.Equivalent(entity.Posts, got.Posts);
    }

    private static FakeDbContext GetFakeDbContext(
        string dbName = nameof(BaseRepositoryTests),
        bool ensureDeleted = true)
    {
        var options = new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase(dbName)
            .Options;
        var context = new FakeDbContext(options);
        if (ensureDeleted)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        return context;
    }

    [Fact]
    public async Task GetEntityAsync_StringBasedInclude_NotNullAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost())
                    .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var got = await repository.GetAsync(entity.Id, new List<string>() { nameof(entity.Posts) });

        // Assert
        Assert.NotNull(got);
        Assert.Equivalent(entity.Posts, got.Posts);
    }

    [Fact]
    public async Task GetEntityAsync_ThenInclude_NotNullAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost())
                    .HasManyForEachEntity(x => x.Tags, new EntityGenerator<FakeTag>(new FakeTag()))
                    .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var got = await repository.GetAsync(entity.Id, new List<string>() { "Posts.Tags" });

        // Assert
        Assert.NotNull(got);
        Assert.Equivalent(entity.Posts, got.Posts);
    }

    [Fact]
    public async Task GetRangeAsync_Keys_ReturnMatchedEntitiesAsync()
    {
        // Arrange
        var blogs = new List<FakeBlog> { new() { Title = "1" }, new() { Title = "2" }, new() { Title = "3" } };
        var db = GetFakeDbContext();
        await db.AddRangeAsync(blogs);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var got = await repository.GetRangeAsync(blogs.Take(2).Select(x => x.Id));

        // Assert
        Assert.Equivalent(blogs.Take(2).ToList(), got);
    }

    [Fact]
    public async Task GetRangeAsync_KeysNotInStore_ReturnEmptyListAsync()
    {
        // Arrange
        var db = GetFakeDbContext();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var got = await repository.GetRangeAsync([1, 2]);

        // Assert
        Assert.Empty(got);
    }

    [Fact]
    public async Task GetRangeAsync_Include_LoadsNavigationAsync()
    {
        // Arrange
        var blogs = new List<FakeBlog> { new() { Posts = [new FakePost()] }, new() { Posts = [new FakePost()] } };
        const string dbName = nameof(GetRangeAsync_Include_LoadsNavigationAsync);
        var seedingDb = GetFakeDbContext(dbName);
        await seedingDb.AddRangeAsync(blogs);
        await seedingDb.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), GetFakeDbContext(dbName, false));

        // Act
        var got = await repository.GetRangeAsync(blogs.Select(x => x.Id), x => x.Posts);

        // Assert
        Assert.Equivalent(blogs.Select(x => x.Id).ToList(), got.Select(x => x.Id).ToList());
        Assert.All(got, x => Assert.NotEmpty(x.Posts));
    }

    [Fact]
    public async Task GetRangeAsync_StringBasedInclude_LoadsNavigationAsync()
    {
        // Arrange
        var blogs = new List<FakeBlog> { new() { Posts = [new FakePost()] }, new() { Posts = [new FakePost()] } };
        const string dbName = nameof(GetRangeAsync_StringBasedInclude_LoadsNavigationAsync);
        var seedingDb = GetFakeDbContext(dbName);
        await seedingDb.AddRangeAsync(blogs);
        await seedingDb.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), GetFakeDbContext(dbName, false));

        // Act
        var got = await repository.GetRangeAsync(blogs.Select(x => x.Id), new List<string> { nameof(FakeBlog.Posts) });

        // Assert
        Assert.Equivalent(blogs.Select(x => x.Id).ToList(), got.Select(x => x.Id).ToList());
        Assert.All(got, x => Assert.NotEmpty(x.Posts));
    }

    [Fact]
    public async Task DeleteAsync_SoftDelete_SetDeletedAsync()
    {
        // Arrange
        var db = GetFakeDbContext();
        var blog = new FakeBlog();
        await db.AddAsync(blog);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var deleted = await repository.DeleteAsync(blog);

        // Assert
        Assert.True(deleted.Deleted);
        Assert.Equal(EntityState.Detached, db.Entry(blog).State);
        var stored = await db.Set<FakeBlog>().AsNoTracking().SingleAsync(x => x.Id == blog.Id);
        Assert.True(stored.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_HardDelete_RemoveEntityAsync()
    {
        // Arrange
        var db = GetFakeDbContext();
        var blog = new FakeBlog();
        await db.AddAsync(blog);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var deleted = await repository.DeleteAsync(blog, softDelete: false);

        // Assert
        Assert.False(deleted.Deleted);
        Assert.Equal(EntityState.Detached, db.Entry(blog).State);
        var stored = await db.Set<FakeBlog>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == blog.Id);
        Assert.Null(stored);
    }

    [Fact]
    public async Task DeleteAsync_UntrackedEntity_SoftDeleteStillSavedAsync()
    {
        // Arrange
        const string dbName = nameof(DeleteAsync_UntrackedEntity_SoftDeleteStillSavedAsync);
        var seedingDb = GetFakeDbContext(dbName);
        var blog = new FakeBlog();
        await seedingDb.AddAsync(blog);
        await seedingDb.SaveChangesAsync();
        var db = GetFakeDbContext(dbName, false);
        var repository = new TestRepository(Substitute.For<IMediator>(), db);
        var untracked = new FakeBlog { Id = blog.Id };

        // Act
        await repository.DeleteAsync(untracked);

        // Assert
        Assert.Equal(EntityState.Detached, db.Entry(untracked).State);
        var stored = await db.Set<FakeBlog>().AsNoTracking().SingleAsync(x => x.Id == blog.Id);
        Assert.True(stored.Deleted);
    }

    [Fact]
    public async Task DeleteRangeAsync_SoftDelete_SetDeletedAsync()
    {
        // Arrange
        var blogs = new List<FakeBlog> { new(), new(), new() };
        var db = GetFakeDbContext();
        await db.AddRangeAsync(blogs);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var deleted = await repository.DeleteRangeAsync(blogs.Take(2));

        // Assert
        Assert.Equal(2, deleted.Count);
        Assert.All(deleted, x => Assert.True(x.Deleted));
        Assert.All(deleted, x => Assert.Equal(EntityState.Detached, db.Entry(x).State));
        var stored = await db.Set<FakeBlog>().AsNoTracking().ToListAsync();
        Assert.Equal(2, stored.Count(x => x.Deleted));
        Assert.Single(stored, x => x.Deleted == false);
    }

    [Fact]
    public async Task DeleteRangeAsync_HardDelete_RemoveEntitiesAsync()
    {
        // Arrange
        var blogs = new List<FakeBlog> { new(), new(), new() };
        var db = GetFakeDbContext();
        await db.AddRangeAsync(blogs);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        var deleted = await repository.DeleteRangeAsync(blogs.Take(2), softDelete: false);

        // Assert
        Assert.Equal(2, deleted.Count);
        Assert.All(deleted, x => Assert.Equal(EntityState.Detached, db.Entry(x).State));
        var stored = await db.Set<FakeBlog>().AsNoTracking().ToListAsync();
        var storedIds = stored.Select(x => x.Id).ToList();
        Assert.Equivalent(blogs.Skip(2).Select(x => x.Id).ToList(), storedIds);
    }

    [Fact]
    public async Task DeleteAsync_EntityNotSoftDeletable_ThrowAsync()
    {
        // Arrange
        var repository = new TestArchiveRepository(Substitute.For<IMediator>(), GetFakeDbContext());

        // Act
        var act = () => repository.DeleteAsync(new FakeArchive());

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task DeleteRangeAsync_EntityNotSoftDeletable_ThrowAsync()
    {
        // Arrange
        var repository = new TestArchiveRepository(Substitute.For<IMediator>(), GetFakeDbContext());

        // Act
        var act = () => repository.DeleteRangeAsync([new FakeArchive()]);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task SaveEntitiesAsync_CallBeforeUpdateForRelatedEntity_UpdateDateUpdatedAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost())
                    .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Substitute.For<IMediator>(), db);

        // Act
        entity.Title = "new title";
        entity.Posts.ForEach(x => x.Title = "new title");
        await repository.UpdateAsync(entity);

        // Assert
        Assert.True(entity.DateUpdated > DateTimeOffset.Now.AddDays(-1));
        Assert.All(entity.Posts, post => Assert.True(post.DateUpdated > DateTimeOffset.Now.AddDays(-1)));
    }

    [Fact]
    public async Task SaveEntitiesAsync_DispatchEntityDomainEvents_DispatchAllAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost()).Setup(x
                    => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        db.Add(entity);
        await db.SaveChangesAsync();
        var mediator = Substitute.For<IMediator>();
        var repository = new TestRepository(mediator, db);

        // Act
        entity.Title = "new title";
        entity.AddDomainEvent(new FakeDomainEvent(entity.Id, 1));
        entity.AddDomainEvent(id => new FakeDomainEvent(id, 2));
        await repository.UpdateAsync(entity);

        // Assert
        await mediator.Received(1).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).FakeValue == 1),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).FakeValue == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveEntitiesAsync_DispatchRelatedEntityDomainEvents_DispatchAllAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost()).Setup(x
                    => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        db.Add(entity);
        await db.SaveChangesAsync();
        var mediator = Substitute.For<IMediator>();
        var repository = new TestRepository(mediator, db);

        // Act
        entity.Title = "new title";
        var firstPost = entity.Posts.First();
        firstPost.AddDomainEvent(new FakeDomainEvent(firstPost.Id, 1));
        firstPost.AddDomainEvent(id => new FakeDomainEvent(id, 2));
        await repository.UpdateAsync(entity);

        // Assert
        await mediator.Received(1).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).FakeValue == 1),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).FakeValue == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveEntitiesAsync_DispatchEntityDomainEventsWithGeneratedId_DispatchAllAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost()).Setup(x
                    => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        var mediator = Substitute.For<IMediator>();
        var repository = new TestRepository(mediator, db);

        // Act
        entity.AddDomainEvent(id => new FakeDomainEvent(id, 1));
        entity.Posts.ForEach(x => x.AddDomainEvent(id => new FakeDomainEvent(id, 2)));
        await repository.AddAsync(entity);

        // Assert
        await mediator.Received(1).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).Id != 0 && ((FakeDomainEvent)d).FakeValue == 1),
            Arg.Any<CancellationToken>());
        await mediator.Received(entity.Posts.Count).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).Id != 0 && ((FakeDomainEvent)d).FakeValue == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveEntitiesAsync_DispatchEntityDomainEventsWithMultipleExceptions_ThrowAggregateExceptionsAsync()
    {
        // Arrange
        var entity = new EntityGenerator<FakeBlog>(new FakeBlog())
            .Setup(x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1))
            .HasManyForEachEntity(
                x => x.Posts,
                x => x.Blog,
                new EntityGenerator<FakePost>(new FakePost()).Setup(x
                    => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = GetFakeDbContext();
        var mediator = Substitute.For<IMediator>();
        mediator.Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<ArgumentException>();
        var repository = new TestRepository(mediator, db);

        // Act
        entity.AddDomainEvent(id => new FakeDomainEvent(id, 1));
        entity.Posts.ForEach(x => x.AddDomainEvent(id => new FakeDomainEvent(id, 2)));
        var act = async () => await repository.AddAsync(entity);

        // Assert
        var eventCount = 1 + entity.Posts.Count;
        var exception = await Assert.ThrowsAsync<AggregateException>(act);
        Assert.Equal(eventCount, exception.InnerExceptions.Count);
        await mediator.Received(1).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).Id != 0 && ((FakeDomainEvent)d).FakeValue == 1),
            Arg.Any<CancellationToken>());
        await mediator.Received(entity.Posts.Count).Publish(
            Arg.Is<IDomainEvent>(d => ((FakeDomainEvent)d!).Id != 0 && ((FakeDomainEvent)d).FakeValue == 2),
            Arg.Any<CancellationToken>());
    }
}
