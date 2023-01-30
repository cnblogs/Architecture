using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public record GetStringQuery() : IQuery<string>;