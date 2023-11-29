using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Domain.Events;
using MediatR;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public class CommandHandlers(IMediator mediator)
    : ICommandHandler<CreateCommand, string, TestError>, ICommandHandler<UpdateCommand, string, TestError>,
        ICommandHandler<DeleteCommand, string, TestError>
{
    /// <inheritdoc />
    public async Task<CommandResponse<string, TestError>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        await mediator.Publish(new StringCreatedDomainEvent(request.Data ?? string.Empty), cancellationToken);
        return request.NeedError
                ? CommandResponse<string, TestError>.Fail(TestError.Default)
                : CommandResponse<string, TestError>.Success("create success");
    }

    /// <inheritdoc />
    public Task<CommandResponse<string, TestError>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            request.NeedExecutionError
                ? CommandResponse<string, TestError>.Fail(TestError.Default)
                : CommandResponse<string, TestError>.Success("update success"));
    }

    /// <inheritdoc />
    public Task<CommandResponse<string, TestError>> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            request.NeedError
                ? CommandResponse<string, TestError>.Fail(TestError.Default)
                : CommandResponse<string, TestError>.Success("delete success"));
    }
}
