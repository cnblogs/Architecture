using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;
using MongoDB.Driver;
using Moq;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class FakeMongoDbContext : MongoContext
{
    public Mock<IMongoDatabase> MongoDatabaseMock { get; }
    public Mock<IMongoClient> MongoClientMock { get; }
    public Mock<IClientSessionHandle> ClientSessionHandleMock { get; }
    public Mock<IMongoContextOptions> OptionsMock { get; }

    public FakeMongoDbContext()
        : this(MockOptions())
    {
    }

    public FakeMongoDbContext(Mock<IMongoContextOptions> mockOptionsMock)
        : this(mockOptionsMock, MockDatabase(mockOptionsMock))
    {
    }

    public FakeMongoDbContext(Mock<IMongoContextOptions> mockOptionsMock, Mock<IMongoDatabase> mongoDatabase)
        : base(mockOptionsMock.Object)
    {
        OptionsMock = mockOptionsMock;
        MongoDatabaseMock = mongoDatabase;
        ClientSessionHandleMock = new Mock<IClientSessionHandle>();
        MongoClientMock = new Mock<IMongoClient>();
        MongoClientMock
            .Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClientSessionHandleMock.Object);
        MongoDatabaseMock.Setup(x => x.Client).Returns(MongoClientMock.Object);
        MongoDatabaseMock
            .Setup(x => x.GetCollection<FakeBlog>(It.IsAny<string>(), null))
            .Returns(Mock.Of<IMongoCollection<FakeBlog>>());
    }

    /// <inheritdoc />
    protected override void ConfigureModels(MongoModelBuilder builder)
    {
        builder.Entity<FakeBlog>("fakeBlog");
    }

    private static Mock<IMongoContextOptions> MockOptions()
    {
        return new Mock<IMongoContextOptions>();
    }

    private static Mock<IMongoDatabase> MockDatabase(Mock<IMongoContextOptions> mongoContextOptionsMock)
    {
        var mock = new Mock<IMongoDatabase>();
        mongoContextOptionsMock.Setup(x => x.GetDatabase()).Returns(mock.Object);
        return mock;
    }
}