using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public record GetStringQuery(string? AppId = null, int? StringId = null, bool Found = true) : IQuery<string>;
