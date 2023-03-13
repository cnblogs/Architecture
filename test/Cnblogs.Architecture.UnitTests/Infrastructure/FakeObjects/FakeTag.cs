using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class FakeTag : Entity<int>
{
    public int BlogId { get; set; }
    public int PostId { get; set; }
}
