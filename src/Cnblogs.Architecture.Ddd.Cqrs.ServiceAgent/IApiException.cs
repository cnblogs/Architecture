using System.Diagnostics.CodeAnalysis;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
///     Defines exceptions threw when doing an API call.
/// </summary>
/// <typeparam name="TException">The type of this API exception.</typeparam>
public interface IApiException<out TException>
    where TException : Exception, IApiException<TException>
{
    /// <summary>
    ///     The HTTP status code, -1 if not applied.
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    ///     The raw error message.
    /// </summary>
    string Message { get; }

    /// <summary>
    ///     The error message to display, can be null if such message is not available.
    /// </summary>
    string? UserFriendlyMessage { get; }

    /// <summary>
    ///     Throw a <see cref="TException"/>.
    /// </summary>
    /// <param name="statusCode">HTTP status code, -1 if not available.</param>
    /// <param name="message">The error message.</param>
    /// <param name="userFriendlyMessage">The error message to display, can be null if such message is not available.</param>
    [DoesNotReturn]
    static abstract void Throw(int statusCode = -1, string message = "", string? userFriendlyMessage = null);

    /// <summary>
    ///    Create(but not throw) a <see cref="TException"/>.
    /// </summary>
    /// <param name="statusCode">HTTP status code, -1 if not available.</param>
    /// <param name="message">The error message.</param>
    /// <param name="userFriendlyMessage">The error message to display, can be null if such message is not available.</param>
    /// <returns>A new instance of <see cref="TException"/>.</returns>
    static abstract TException Create(int statusCode = -1, string message = "", string? userFriendlyMessage = null);
}
