using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public class CommandHandlers
    : ICommandHandler<CreateCommand, TestError>, ICommandHandler<UpdateCommand, TestError>,
        ICommandHandler<DeleteCommand, TestError>
{
    /// <inheritdoc />
    public Task<CommandResponse<TestError>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.NeedError
            ? CommandResponse<TestError>.Fail(TestError.Default)
            : CommandResponse<TestError>.Success());
    }

    /// <inheritdoc />
    public Task<CommandResponse<TestError>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.NeedExecutionError
            ? CommandResponse<TestError>.Fail(TestError.Default)
            : CommandResponse<TestError>.Success());
    }

    /// <inheritdoc />
    public Task<CommandResponse<TestError>> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.NeedError
            ? CommandResponse<TestError>.Fail(TestError.Default)
            : CommandResponse<TestError>.Success());
    }
}