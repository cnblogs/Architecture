using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;
using MongoDB.Driver;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class FakeMongoDbContext : MongoContext
{
    public IMongoDatabase MongoDatabaseMock { get; }
    public IMongoClient MongoClientMock { get; }
    public IClientSessionHandle ClientSessionHandleMock { get; }
    public IMongoContextOptions OptionsMock { get; }

    public FakeMongoDbContext()
        : this(MockOptions())
    {
    }

    public FakeMongoDbContext(IMongoContextOptions mockOptionsMock)
        : this(mockOptionsMock, MockDatabase(mockOptionsMock))
    {
    }

    public FakeMongoDbContext(IMongoContextOptions mockOptionsMock, IMongoDatabase mongoDatabase)
        : base(mockOptionsMock)
    {
        OptionsMock = mockOptionsMock;
        MongoDatabaseMock = mongoDatabase;
        ClientSessionHandleMock = Substitute.For<IClientSessionHandle>();
        MongoClientMock = Substitute.For<IMongoClient>();
        MongoClientMock.StartSessionAsync(Arg.Any<ClientSessionOptions>(), Arg.Any<CancellationToken>())
            .Returns(ClientSessionHandleMock);
        MongoDatabaseMock.Client.Returns(MongoClientMock);
        MongoDatabaseMock.GetCollection<FakeBlog>(Arg.Any<string>())
            .Returns(Substitute.For<IMongoCollection<FakeBlog>>());
    }

    /// <inheritdoc />
    protected override void ConfigureModels(MongoModelBuilder builder)
    {
        builder.Entity<FakeBlog>("fakeBlog");
        builder.Entity<FakePost>("fakePost");
        builder.Entity<FakeTag>("fakeTag");
    }

    private static IMongoContextOptions MockOptions()
    {
        return Substitute.For<IMongoContextOptions>();
    }

    private static IMongoDatabase MockDatabase(IMongoContextOptions mongoContextOptionsMock)
    {
        var mock = Substitute.For<IMongoDatabase>();
        mongoContextOptionsMock.GetDatabase().Returns(mock);
        return mock;
    }
}
