using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class ListStringsQueryHandler : IPageableQueryHandler<ListStringsQuery, string>
{
    /// <inheritdoc />
    public async Task<PagedList<string>> Handle(ListStringsQuery request, CancellationToken cancellationToken)
    {
        return new PagedList<string>(new[] { "hello" });
    }
}