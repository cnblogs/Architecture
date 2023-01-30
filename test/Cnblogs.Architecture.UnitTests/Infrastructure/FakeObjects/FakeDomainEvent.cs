using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public record FakeDomainEvent(int Id, int FakeValue) : DomainEvent;