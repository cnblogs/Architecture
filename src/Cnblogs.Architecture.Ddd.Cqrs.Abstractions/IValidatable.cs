namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a request that can be validated.
/// </summary>
public interface IValidatable
{
    /// <summary>
    ///     Validate the object, validate will pass if <paramref name="validationErrors"/> is empty.
    /// </summary>
    /// <param name="validationErrors">The validation error collection.</param>
    void Validate(ValidationErrors validationErrors);
}
