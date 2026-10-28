using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using MediatR;
using MongoDB.Driver;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.MongoDb;

public class MongoBaseRepositoryTests
{
    [Fact]
    public async Task AddAsync_WithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var response = await repository.AddAsync(blog);

        // Assert
        Assert.NotNull(response);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(1)
            .InsertOneAsync(
                Arg.Any<FakeBlog>(),
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(1).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddRangeAsync_WithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blogs = Enumerable.Range(0, 10).Select(_ => new FakeBlog()).ToList();
        blogs.ForEach(x => x.AddDomainEvent(new FakeDomainEvent(1, 1)));

        // Act
        var response = await repository.AddRangeAsync(blogs);

        // Assert
        Assert.Equal(blogs.Count, response.Count);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(1)
            .InsertManyAsync(
                Arg.Any<IEnumerable<FakeBlog>>(),
                Arg.Any<InsertManyOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(blogs.Count)
            .Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_SoftDeleteWithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var response = await repository.DeleteAsync(blog);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Deleted);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(1)
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<FakeBlog>>(),
                Arg.Any<FakeBlog>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(0)
            .DeleteOneAsync(Arg.Any<FilterDefinition<FakeBlog>>(), Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_HardDeleteWithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var response = await repository.DeleteAsync(blog, softDelete: false);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Deleted);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(1)
            .DeleteOneAsync(Arg.Any<FilterDefinition<FakeBlog>>(), Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRangeAsync_SoftDeleteWithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blogs = Enumerable.Range(0, 10).Select(_ => new FakeBlog()).ToList();
        blogs.ForEach(x => x.AddDomainEvent(new FakeDomainEvent(1, 1)));

        // Act
        var response = await repository.DeleteRangeAsync(blogs);

        // Assert
        Assert.Equal(blogs.Count, response.Count);
        Assert.All(response, x => Assert.True(x.Deleted));
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .BulkWriteAsync(
                Arg.Any<IEnumerable<WriteModel<FakeBlog>>>(),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(0)
            .DeleteManyAsync(Arg.Any<FilterDefinition<FakeBlog>>(), Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(blogs.Count)
            .Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRangeAsync_HardDeleteWithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blogs = Enumerable.Range(0, 10).Select(_ => new FakeBlog()).ToList();
        blogs.ForEach(x => x.AddDomainEvent(new FakeDomainEvent(1, 1)));

        // Act
        var response = await repository.DeleteRangeAsync(blogs, softDelete: false);

        // Assert
        Assert.Equal(blogs.Count, response.Count);
        Assert.All(response, x => Assert.False(x.Deleted));
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .DeleteManyAsync(Arg.Any<FilterDefinition<FakeBlog>>(), Arg.Any<CancellationToken>());
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(0)
            .BulkWriteAsync(
                Arg.Any<IEnumerable<WriteModel<FakeBlog>>>(),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(blogs.Count)
            .Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_EntityNotSoftDeletable_ThrowAsync()
    {
        // Arrange
        var repository = new MongoBaseRepository<FakeMongoDbContext, FakeArchive, int>(
            new FakeMongoDbContext(),
            Substitute.For<IMediator>());

        // Act
        var act = () => repository.DeleteAsync(new FakeArchive());

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task DeleteRangeAsync_EntityNotSoftDeletable_ThrowAsync()
    {
        // Arrange
        var repository = new MongoBaseRepository<FakeMongoDbContext, FakeArchive, int>(
            new FakeMongoDbContext(),
            Substitute.For<IMediator>());

        // Act
        var act = () => repository.DeleteRangeAsync([new FakeArchive()]);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_WithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var response = await repository.UpdateAsync(blog);

        // Assert
        Assert.NotNull(response);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(1)
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<FakeBlog>>(),
                Arg.Any<FakeBlog>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRangeAsync_WithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blogs = Enumerable.Range(0, 10).Select(_ => new FakeBlog()).ToList();
        blogs.ForEach(x => x.AddDomainEvent(new FakeDomainEvent(1, 1)));

        // Act
        var response = await repository.UpdateRangeAsync(blogs);

        // Assert
        Assert.Equal(blogs.Count, response.Count());
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .BulkWriteAsync(
                Arg.Any<IEnumerable<WriteModel<FakeBlog>>>(),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(blogs.Count)
            .Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Uow_Insert_CommitAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var uow = repository.UnitOfWork;
        uow.Add(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        Assert.True(response);
        await repository.MongoDbContext.ClientSessionHandleMock.Received(1)
            .CommitTransactionAsync(Arg.Any<CancellationToken>());
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .BulkWriteAsync(
                Arg.Any<IClientSessionHandle>(),
                Arg.Any<IEnumerable<WriteModel<FakeBlog>>>(),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(1).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Uow_InsertThenRemove_NoChangeAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var uow = repository.UnitOfWork;
        uow.Add(blog);
        uow.Delete(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        Assert.True(response);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(0)
            .BulkWriteAsync(
                Arg.Any<IClientSessionHandle>(),
                Arg.Any<IEnumerable<WriteModel<FakeBlog>>>(),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(0).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Uow_DeleteThenInsert_UpdateAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var uow = repository.UnitOfWork;
        uow.Delete(blog);
        uow.Add(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        Assert.True(response);
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .BulkWriteAsync(
                Arg.Any<IClientSessionHandle>(),
                Arg.Is<IEnumerable<WriteModel<FakeBlog>>>(y => y!.Any(z => z is ReplaceOneModel<FakeBlog>)),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(1).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Uow_DeleteThenUpdate_Throw()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var uow = repository.UnitOfWork;
        uow.Delete(blog);
        var act = () => uow.Update(blog);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Uow_UpdateThenInsert_Throw()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var uow = repository.UnitOfWork;
        uow.Update(blog);
        var act = () => uow.Add(blog);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public async Task Uow_DeleteWithDomainEvent_CommitAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var uow = repository.UnitOfWork;
        uow.Delete(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        Assert.True(response);
        await repository.MongoDbContext.ClientSessionHandleMock.Received(1)
            .CommitTransactionAsync(Arg.Any<CancellationToken>());
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .BulkWriteAsync(
                Arg.Any<IClientSessionHandle>(),
                Arg.Any<IEnumerable<WriteModel<FakeBlog>>>(),
                Arg.Any<BulkWriteOptions>(),
                Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received(1).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }
}
