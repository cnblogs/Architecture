using Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;
using MediatR;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class TestMongoRepository : MongoBaseRepository<FakeMongoDbContext, FakeBlog, int>
{
    public IMediator MediatorMock { get; }
    public FakeMongoDbContext MongoDbContext { get; }

    public TestMongoRepository()
        : this(new FakeMongoDbContext(), Substitute.For<IMediator>())
    {
    }

    public TestMongoRepository(FakeMongoDbContext fakeDbContext, IMediator mediator)
        : base(fakeDbContext, mediator)
    {
        MongoDbContext = fakeDbContext;
        MediatorMock = mediator;
    }
}
