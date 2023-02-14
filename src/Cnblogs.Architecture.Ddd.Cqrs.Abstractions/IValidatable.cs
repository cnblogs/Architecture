namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a request that can be validated.
/// </summary>
public interface IValidatable
{
    /// <summary>
    ///     Validate the object, return <see cref="ValidationError"/> if fails or <code>null</code> if passed.
    /// </summary>
    ValidationError? Validate();
}