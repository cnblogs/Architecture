namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Configure the response type for command errors.
/// </summary>
public enum ErrorResponseType
{
    /// <summary>
    ///     Returns plain text, this is the default behavior.
    /// </summary>
    PlainText,

    /// <summary>
    ///     Returns <see cref="ProblemDetails"/>.
    /// </summary>
    ProblemDetails,

    /// <summary>
    ///     Handles command error by custom logic.
    /// </summary>
    Custom
}
