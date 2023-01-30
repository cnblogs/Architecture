using Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;

using MediatR;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class TestRepository : BaseRepository<FakeDbContext, FakeBlog, int>
{
    public TestRepository(IMediator mediator, FakeDbContext context)
        : base(mediator, context)
    {
    }
}