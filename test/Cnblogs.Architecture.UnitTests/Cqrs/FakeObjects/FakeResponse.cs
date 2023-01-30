using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class FakeResponse : IValidationResponse
{
    /// <inheritdoc />
    public bool IsValidationError { get; init; }

    /// <inheritdoc />
    public string ErrorMessage { get; init; } = string.Empty;

    /// <inheritdoc />
    public ValidationError? ValidationError { get; init; }
}