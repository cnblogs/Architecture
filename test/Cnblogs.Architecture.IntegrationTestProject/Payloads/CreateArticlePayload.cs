using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

namespace Cnblogs.Architecture.IntegrationTestProject.Payloads;

public record CreateArticlePayload([property: Trimmed] string Title);
