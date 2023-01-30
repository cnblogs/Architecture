using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class GetStringQueryHandler : IQueryHandler<GetStringQuery, string>
{
    /// <inheritdoc />
    public async Task<string?> Handle(GetStringQuery request, CancellationToken cancellationToken)
    {
        return "Hello";
    }
}