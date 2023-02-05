using System.Diagnostics.CodeAnalysis;

namespace Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;

/// <summary>
///     API 异常接口
/// </summary>
/// <typeparam name="TException">异常类型。</typeparam>
public interface IApiException<out TException>
    where TException : Exception, IApiException<TException>
{
    /// <summary>
    ///     HTTP 状态码，不适用则为 -1。
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    ///     错误信息。
    /// </summary>
    string Message { get; }

    /// <summary>
    ///     显示给用户的错误信息。
    /// </summary>
    string? UserFriendlyMessage { get; }

    /// <summary>
    ///     抛出异常。
    /// </summary>
    /// <param name="statusCode">HTTP 状态码，若不适用则为 -1。</param>
    /// <param name="message">错误信息。</param>
    /// <param name="userFriendlyMessage">给用户显示的错误信息。</param>
    [DoesNotReturn]
    static abstract void Throw(int statusCode = -1, string message = "", string? userFriendlyMessage = null);

    /// <summary>
    ///    创建异常。
    /// </summary>
    /// <param name="statusCode">HTTP 状态码，若不适用则为 -1。</param>
    /// <param name="message">错误信息。</param>
    /// <param name="userFriendlyMessage">给用户显示的错误信息。</param>
    /// <returns></returns>
    static abstract TException Create(int statusCode = -1, string message = "", string? userFriendlyMessage = null);
}