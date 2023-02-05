﻿using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Queries;

public class GetStringQueryHandler : IQueryHandler<GetStringQuery, string>
{
    /// <inheritdoc />
    public Task<string?> Handle(GetStringQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult((string?)"Hello");
    }
}