using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public record TestPageableQuery(bool? Deleted, PagingParams? PagingParams, string? OrderByString)
    : IPageableQuery<FakePostDto>;
