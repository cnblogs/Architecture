using Cnblogs.Architecture.Ddd.EventBus.Abstractions;

namespace Cnblogs.Architecture.TestIntegrationEvents;

public record TestIntegrationEvent(Guid Id, DateTimeOffset CreatedTime, string Message) : IntegrationEvent(Id, CreatedTime);