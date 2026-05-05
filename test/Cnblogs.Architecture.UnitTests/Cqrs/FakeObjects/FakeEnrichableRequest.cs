using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using MediatR;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class FakeEnrichableRequest<TResponse> : IEnrichableRequest, IRequest<TResponse>
    where TResponse : class
{
    public bool SkipEnrich { get; set; }

    /// <inheritdoc />
    public bool IsEnrichSkipped() => SkipEnrich;
}