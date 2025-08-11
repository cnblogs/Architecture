using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_ValidationFailed_ReturnObjectAsync()
    {
        // Arrange
        var error = new ValidationError("failed", "parameter");
        var request = new FakeQuery<FakeResponse>(() => error);
        var behavior = new ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>(
            NullLogger<ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>>.Instance);

        // Act
        var result = await behavior.Handle(request, _ => Task.FromResult(new FakeResponse()), CancellationToken.None);

        // Assert
        var errors = new ValidationErrors { error };
        Assert.Equivalent(new { IsValidationError = true, ValidationErrors = errors }, result);
    }

    [Fact]
    public async Task ValidationBehavior_ValidationSuccess_ReturnNextAsync()
    {
        // Arrange
        var request = new FakeQuery<FakeResponse>(() => null);
        var behavior = new ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>(
            NullLogger<ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>>.Instance);

        // Act
        var result = await behavior.Handle(request, _ => Task.FromResult(new FakeResponse()), CancellationToken.None);

        // Assert
        Assert.Equivalent(new { IsValidationError = false, ValidationErrors = new ValidationErrors() }, result);
    }
}
