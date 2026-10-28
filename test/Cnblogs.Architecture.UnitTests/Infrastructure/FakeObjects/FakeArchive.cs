using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

/// <summary>
///     仅继承 <see cref="EntityBase" /> 的实体，用于测试不支持软删除的类型。
/// </summary>
public class FakeArchive : EntityBase, IEntity<int>, IAggregateRoot
{
    public int Id { get; set; }
}
