namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents response for <see cref="IValidatable" />.
/// </summary>
public interface IValidationResponse
{
    /// <summary>
    ///     Indicates whether validation is failed.
    /// </summary>
    bool IsValidationError { get; init; }

    /// <summary>
    ///     Contain error message if validation fails.
    /// </summary>
    string ErrorMessage { get; init; }

    /// <summary>
    ///     The validation results, null if validation was passed.
    /// </summary>
    ValidationError? ValidationError { get; init; }
}