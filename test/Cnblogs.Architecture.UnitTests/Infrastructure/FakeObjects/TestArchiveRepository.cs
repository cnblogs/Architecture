using Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;
using MediatR;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class TestArchiveRepository : BaseRepository<FakeDbContext, FakeArchive, int>
{
    public TestArchiveRepository(IMediator mediator, FakeDbContext context)
        : base(mediator, context)
    {
    }
}
