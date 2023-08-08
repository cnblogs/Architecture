using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Domain.Events;

public record StringCreatedDomainEvent(string Data) : DomainEvent;
