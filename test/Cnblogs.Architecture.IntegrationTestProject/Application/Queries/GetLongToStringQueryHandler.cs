using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class GetLongToStringQueryHandler : IQueryHandler<GetLongToStringQuery, LongToStringModel>
{
    /// <inheritdoc />
    public async Task<LongToStringModel?> Handle(GetLongToStringQuery request, CancellationToken cancellationToken)
    {
        return new LongToStringModel() { Id = request.Id };
    }
}
