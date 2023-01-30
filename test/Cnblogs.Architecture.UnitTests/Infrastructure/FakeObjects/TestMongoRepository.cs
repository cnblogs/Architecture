using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;
using MediatR;
using Moq;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class TestMongoRepository : MongoBaseRepository<FakeMongoDbContext, FakeBlog, int>
{
    public Mock<IMediator> MediatorMock { get; }
    public FakeMongoDbContext MongoDbContext { get; }

    public TestMongoRepository()
        : this(new FakeMongoDbContext(), new Mock<IMediator>())
    {
    }

    public TestMongoRepository(FakeMongoDbContext fakeDbContext, Mock<IMediator> mediator)
        : base(fakeDbContext, mediator.Object)
    {
        MongoDbContext = fakeDbContext;
        MediatorMock = mediator;
    }
}