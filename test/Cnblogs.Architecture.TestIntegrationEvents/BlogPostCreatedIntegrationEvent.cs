using Cnblogs.Architecture.Ddd.EventBus.Abstractions;

namespace Cnblogs.Architecture.TestIntegrationEvents;

public record BlogPostCreatedIntegrationEvent(Guid Id, DateTimeOffset CreatedTime, string Title) : IntegrationEvent(Id, CreatedTime);