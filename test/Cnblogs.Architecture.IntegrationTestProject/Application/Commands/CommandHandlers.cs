using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Domain.Events;
using MediatR;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public class CommandHandlers
    : ICommandHandler<CreateCommand, TestError>, ICommandHandler<UpdateCommand, TestError>,
        ICommandHandler<DeleteCommand, TestError>
{
    private readonly IMediator _mediator;

    public CommandHandlers(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<CommandResponse<TestError>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Publish(new StringCreatedDomainEvent(request.Data ?? string.Empty), cancellationToken);
        return request.NeedError
                ? CommandResponse<TestError>.Fail(TestError.Default)
                : CommandResponse<TestError>.Success();
    }

    /// <inheritdoc />
    public Task<CommandResponse<TestError>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            request.NeedExecutionError
                ? CommandResponse<TestError>.Fail(TestError.Default)
                : CommandResponse<TestError>.Success());
    }

    /// <inheritdoc />
    public Task<CommandResponse<TestError>> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            request.NeedError
                ? CommandResponse<TestError>.Fail(TestError.Default)
                : CommandResponse<TestError>.Success());
    }
}
