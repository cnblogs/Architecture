using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class FakeBlog : Entity<int>, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;

    // navigations
    public List<FakePost> Posts { get; set; } = null!;
}
