using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public class CommandHandlers
    : ICommandHandler<CreateCommand, TestError>, ICommandHandler<UpdateCommand, TestError>,
        ICommandHandler<DeleteCommand, TestError>
{
    /// <inheritdoc />
    public async Task<CommandResponse<TestError>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        return request.NeedError
            ? CommandResponse<TestError>.Fail(TestError.Default)
            : CommandResponse<TestError>.Success();
    }

    /// <inheritdoc />
    public async Task<CommandResponse<TestError>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        return request.NeedError
            ? CommandResponse<TestError>.Fail(TestError.Default)
            : CommandResponse<TestError>.Success();
    }

    /// <inheritdoc />
    public async Task<CommandResponse<TestError>> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        return request.NeedError
            ? CommandResponse<TestError>.Fail(TestError.Default)
            : CommandResponse<TestError>.Success();
    }
}