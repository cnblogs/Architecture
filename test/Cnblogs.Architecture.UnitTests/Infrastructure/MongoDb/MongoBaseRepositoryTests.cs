using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using FluentAssertions;
using MongoDB.Driver;
using Moq;

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
        response.Should().NotBeNull();
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty)).Verify(
            x => x.InsertOneAsync(It.IsAny<FakeBlog>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
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
        response.Should().HaveSameCount(blogs);
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.InsertManyAsync(
                    It.IsAny<IEnumerable<FakeBlog>>(),
                    It.IsAny<InsertManyOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(blogs.Count));
    }

    [Fact]
    public async Task DeleteAsync_WithDomainEvent_SaveThenPublishAsync()
    {
        // Arrange
        var repository = new TestMongoRepository();
        var blog = new FakeBlog();
        blog.AddDomainEvent(new FakeDomainEvent(1, 1));

        // Act
        var response = await repository.DeleteAsync(blog);

        // Assert
        response.Should().NotBeNull();
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.DeleteOneAsync(It.IsAny<FilterDefinition<FakeBlog>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()));
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
        response.Should().NotBeNull();
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<FakeBlog>>(),
                    It.IsAny<FakeBlog>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()));
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
        response.Should().HaveSameCount(blogs);
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.BulkWriteAsync(
                    It.IsAny<IEnumerable<WriteModel<FakeBlog>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(blogs.Count));
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
        uow.Insert(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        response.Should().BeTrue();
        repository.MongoDbContext.ClientSessionHandleMock.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.BulkWriteAsync(
                    It.IsAny<IClientSessionHandle>(),
                    It.IsAny<IEnumerable<WriteModel<FakeBlog>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
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
        uow.Insert(blog);
        uow.Remove(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        response.Should().BeTrue();
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.BulkWriteAsync(
                    It.IsAny<IClientSessionHandle>(),
                    It.IsAny<IEnumerable<WriteModel<FakeBlog>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
        uow.Remove(blog);
        uow.Insert(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        response.Should().BeTrue();
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.BulkWriteAsync(
                    It.IsAny<IClientSessionHandle>(),
                    It.Is<IEnumerable<WriteModel<FakeBlog>>>(y => y.Any(z => z is ReplaceOneModel<FakeBlog>)),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
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
        uow.Remove(blog);
        var act = () => uow.Update(blog);

        // Assert
        act.Should().Throw<InvalidOperationException>();
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
        var act = () => uow.Insert(blog);

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>();
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
        uow.Remove(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        response.Should().BeTrue();
        repository.MongoDbContext.ClientSessionHandleMock.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        Mock.Get(repository.MongoDbContext.MongoDatabaseMock.Object.GetCollection<FakeBlog>(string.Empty))
            .Verify(
                x => x.BulkWriteAsync(
                    It.IsAny<IClientSessionHandle>(),
                    It.IsAny<IEnumerable<WriteModel<FakeBlog>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        repository.MediatorMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}