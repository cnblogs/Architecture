using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_ValidationFailed_ReturnObjectAsync()
    {
        // Arrange
        var request = new FakeQuery<FakeResponse>(() => new ValidationError("failed", "parameter"));
        var behavior = new ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>(
            NullLogger<ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>>.Instance);

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(new FakeResponse()), default);

        // Assert
        result.Should().BeEquivalentTo(
            new { IsValidationError = true, ValidationError = new ValidationError("failed", "parameter") });
    }

    [Fact]
    public async Task ValidationBehavior_ValidationSuccess_ReturnNextAsync()
    {
        // Arrange
        var request = new FakeQuery<FakeResponse>(() => null);
        var behavior = new ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>(
            NullLogger<ValidationBehavior<FakeQuery<FakeResponse>, FakeResponse>>.Instance);

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(new FakeResponse()), default);

        // Assert
        result.Should().BeEquivalentTo(
            new { IsValidationError = false, ValidationError = (ValidationError?)null });
    }
}