using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public class CreateLongToStringCommandHandler : ICommandHandler<CreateLongToStringCommand, LongToStringModel, TestError>
{
    /// <inheritdoc />
    public Task<CommandResponse<LongToStringModel, TestError>> Handle(
        CreateLongToStringCommand request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(CommandResponse<LongToStringModel, TestError>.Success(new LongToStringModel() { Id = request.Id }));
    }
}
