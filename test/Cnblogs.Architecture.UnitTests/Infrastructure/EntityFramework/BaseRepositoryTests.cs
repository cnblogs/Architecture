using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.TestShared;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

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
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Mock.Of<IMediator>(), db);

        // Act
        var got = await repository.GetAsync(entity.Id, e => e.Posts);

        // Assert
        got.Should().NotBeNull();
        got!.Posts.Should().BeEquivalentTo(entity.Posts);
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
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Mock.Of<IMediator>(), db);

        // Act
        var got = await repository.GetAsync(entity.Id, new List<string>() { nameof(entity.Posts) });

        // Assert
        got.Should().NotBeNull();
        got!.Posts.Should().BeEquivalentTo(entity.Posts);
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
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Mock.Of<IMediator>(), db);

        // Act
        var got = await repository.GetAsync(entity.Id, new List<string>() { "Posts.Tags" });

        // Assert
        got.Should().NotBeNull();
        got!.Posts.Should().BeEquivalentTo(entity.Posts);
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
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        db.Add(entity);
        await db.SaveChangesAsync();
        var repository = new TestRepository(Mock.Of<IMediator>(), db);

        // Act
        entity.Title = "new title";
        entity.Posts.ForEach(x => x.Title = "new title");
        await repository.UpdateAsync(entity);

        // Assert
        entity.DateUpdated.Should().BeAfter(DateTimeOffset.Now.AddDays(-1));
        entity.Posts.Should().AllSatisfy(x => x.DateUpdated.Should().BeAfter(DateTimeOffset.Now.AddDays(-1)));
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
                new EntityGenerator<FakePost>(new FakePost()).Setup(
                    x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        db.Add(entity);
        await db.SaveChangesAsync();
        var mediator = new Mock<IMediator>();
        var repository = new TestRepository(mediator.Object, db);

        // Act
        entity.Title = "new title";
        entity.AddDomainEvent(new FakeDomainEvent(entity.Id, 1));
        entity.AddDomainEvent(id => new FakeDomainEvent(id, 2));
        await repository.UpdateAsync(entity);

        // Assert
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).FakeValue == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).FakeValue == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
                new EntityGenerator<FakePost>(new FakePost()).Setup(
                    x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        db.Add(entity);
        await db.SaveChangesAsync();
        var mediator = new Mock<IMediator>();
        var repository = new TestRepository(mediator.Object, db);

        // Act
        entity.Title = "new title";
        var firstPost = entity.Posts.First();
        firstPost.AddDomainEvent(new FakeDomainEvent(firstPost.Id, 1));
        firstPost.AddDomainEvent(id => new FakeDomainEvent(id, 2));
        await repository.UpdateAsync(entity);

        // Assert
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).FakeValue == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).FakeValue == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
                new EntityGenerator<FakePost>(new FakePost()).Setup(
                    x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        var mediator = new Mock<IMediator>();
        var repository = new TestRepository(mediator.Object, db);

        // Act
        entity.AddDomainEvent(id => new FakeDomainEvent(id, 1));
        entity.Posts.ForEach(x => x.AddDomainEvent(id => new FakeDomainEvent(id, 2)));
        await repository.AddAsync(entity);

        // Assert
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).Id != 0 && ((FakeDomainEvent)d).FakeValue == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).Id != 0 && ((FakeDomainEvent)d).FakeValue == 2),
                It.IsAny<CancellationToken>()),
            Times.Exactly(entity.Posts.Count));
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
                new EntityGenerator<FakePost>(new FakePost()).Setup(
                    x => x.DateUpdated = DateTimeOffset.Now.AddDays(-1)))
            .GenerateSingle();
        var db = new FakeDbContext(
            new DbContextOptionsBuilder<FakeDbContext>().UseInMemoryDatabase("inmemory").Options);
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Throws<ArgumentException>();
        var repository = new TestRepository(mediator.Object, db);

        // Act
        entity.AddDomainEvent(id => new FakeDomainEvent(id, 1));
        entity.Posts.ForEach(x => x.AddDomainEvent(id => new FakeDomainEvent(id, 2)));
        var act = async () => await repository.AddAsync(entity);

        // Assert
        var eventCount = 1 + entity.Posts.Count;
        (await act.Should().ThrowAsync<AggregateException>()).And.InnerExceptions.Should()
            .HaveCount(eventCount);
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).Id != 0 && ((FakeDomainEvent)d).FakeValue == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mediator.Verify(
            x => x.Publish(
                It.Is<IDomainEvent>(d => ((FakeDomainEvent)d).Id != 0 && ((FakeDomainEvent)d).FakeValue == 2),
                It.IsAny<CancellationToken>()),
            Times.Exactly(entity.Posts.Count));
    }
}
