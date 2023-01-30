using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public record ListStringsQuery(PagingParams? PagingParams, string? OrderByString) : IPageableQuery<string>;