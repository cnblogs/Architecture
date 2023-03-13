using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class FakePost : Entity<int>
{
    public int BlogId { get; set; }
    public string Title { get; set; } = string.Empty;

    // navigations
    public FakeBlog Blog { get; set; } = null!;
    public List<FakeTag> Tags { get; set; } = null!;
}
