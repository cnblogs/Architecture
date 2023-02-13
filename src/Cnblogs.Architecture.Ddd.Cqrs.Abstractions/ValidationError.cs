namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     A Validation error returned by <see cref="IValidatable"/>.
/// </summary>
/// <param name="Message">The error message.</param>
/// <param name="ParameterName">The parameter name that failed to validate.</param>
public record ValidationError(string Message, string? ParameterName);