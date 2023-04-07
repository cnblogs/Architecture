using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public record UpdateCommand(
        int Id,
        bool NeedValidationError,
        bool NeedExecutionError,
        bool ValidateOnly = false)
    : ICommand<TestError>, IValidatable
{
    /// <inheritdoc />
    public void Validate(ValidationErrors errors)
    {
        if (NeedValidationError)
        {
            errors.Add(new ValidationError("need validation error", nameof(NeedValidationError)));
        }
    }
}
