using Cnblogs.Architecture.Ddd.EventBus.Abstractions;

namespace Cnblogs.Architecture.TestIntegrationEvents;

public record TestIntegrationEvent(Guid Id, DateTimeOffset CreatedTime) : IntegrationEvent(Id, CreatedTime);