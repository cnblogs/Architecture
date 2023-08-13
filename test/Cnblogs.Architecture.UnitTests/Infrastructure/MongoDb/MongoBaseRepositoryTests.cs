using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using FluentAssertions;
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
        response.Should().NotBeNull();
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
        response.Should().HaveSameCount(blogs);
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
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty)
            .Received(1)
            .DeleteOneAsync(Arg.Any<FilterDefinition<FakeBlog>>(), Arg.Any<CancellationToken>());
        await repository.MediatorMock.Received().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
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
        response.Should().HaveSameCount(blogs);
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
        response.Should().BeTrue();
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
        response.Should().BeTrue();
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
        response.Should().BeTrue();
        await repository.MongoDbContext.MongoDatabaseMock.GetCollection<FakeBlog>(string.Empty).Received(1)
            .BulkWriteAsync(
                Arg.Any<IClientSessionHandle>(),
                Arg.Is<IEnumerable<WriteModel<FakeBlog>>>(y => y.Any(z => z is ReplaceOneModel<FakeBlog>)),
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
        var act = () => uow.Add(blog);

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
        uow.Delete(blog);
        var response = await uow.SaveEntitiesAsync();

        // Assert
        response.Should().BeTrue();
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
