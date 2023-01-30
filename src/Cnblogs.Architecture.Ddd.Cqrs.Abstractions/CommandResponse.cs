using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     命令返回的结果。
/// </summary>
public abstract record CommandResponse : IValidationResponse, ILockableResponse
{
    /// <summary>
    ///     是否出现验证错误。
    /// </summary>
    public bool IsValidationError { get; init; }

    /// <summary>
    /// 是否出现并发错误。
    /// </summary>
    public bool IsConcurrentError { get; init; }

    /// <summary>
    ///     错误信息。
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <inheritdoc />
    public ValidationError? ValidationError { get; init; }

    /// <inheritdoc />
    public bool LockAcquired { get; set; }

    /// <summary>
    ///     执行是否成功。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSuccess()
    {
        return IsValidationError == false && string.IsNullOrEmpty(ErrorMessage) && IsConcurrentError == false;
    }

    /// <summary>
    ///     获取错误信息。
    /// </summary>
    /// <returns></returns>
    public virtual string GetErrorMessage() => ErrorMessage;
}

/// <summary>
///     命令返回的结果。
/// </summary>
/// <typeparam name="TError">错误枚举类型。</typeparam>
public record CommandResponse<TError> : CommandResponse
    where TError : Enumeration
{
    /// <summary>
    ///     构造一个 <see cref="CommandResponse{TError}" />。
    /// </summary>
    public CommandResponse()
    {
        ErrorCode = default;
    }

    /// <summary>
    ///     构造一个 <see cref="CommandResponse{TError}" />。
    /// </summary>
    /// <param name="errorCode">错误码。</param>
    public CommandResponse(TError errorCode)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    ///     错误码。
    /// </summary>
    public TError? ErrorCode { get; set; }

    /// <summary>
    ///     构造一个代表命令执行失败的 <see cref="CommandResponse{TError}" />
    /// </summary>
    /// <param name="errorCode">错误码。</param>
    /// <returns>代表命令执行失败的 <see cref="CommandResponse{TError}" /></returns>
    public static CommandResponse<TError> Fail(TError errorCode)
    {
        return new CommandResponse<TError>(errorCode);
    }

    /// <inheritdoc />
    public override bool IsSuccess()
    {
        return base.IsSuccess() && ErrorCode == null;
    }

    /// <inheritdoc />
    public override string GetErrorMessage()
    {
        return ErrorCode?.Name ?? ErrorMessage;
    }

    /// <summary>
    ///     构造一个代表命令执行成功的 <see cref="CommandResponse{TError}" />。
    /// </summary>
    /// <returns>代表命令执行成功的 <see cref="CommandResponse{TError}" /></returns>
    public static CommandResponse<TError> Success()
    {
        return new CommandResponse<TError>();
    }
}

/// <summary>
///     命令返回的结果。
/// </summary>
/// <typeparam name="TView">命令执行成功时返回的结果类型。</typeparam>
/// <typeparam name="TError">错误类型。</typeparam>
public record CommandResponse<TView, TError> : CommandResponse<TError>, IObjectResponse
    where TError : Enumeration
{
    /// <summary>
    ///     构造一个 <see cref="CommandResponse{TView,TError}" />。
    /// </summary>
    public CommandResponse()
    {
    }

    /// <summary>
    ///     构造一个 <see cref="CommandResponse{TError}" />。
    /// </summary>
    /// <param name="errorCode">错误码。</param>
    public CommandResponse(TError errorCode)
        : base(errorCode)
    {
    }

    /// <summary>
    ///     构造一个 <see cref="CommandResponse{TError}" />。
    /// </summary>
    /// <param name="response">命令返回结果。</param>
    private CommandResponse(TView response)
    {
        Response = response;
    }

    /// <summary>
    ///     命令执行结果。
    /// </summary>
    public TView? Response { get; }

    /// <summary>
    ///     构造一个代表执行失败的 <see cref="CommandResponse{TView,TError}" />。
    /// </summary>
    /// <param name="errorCode">错误码。</param>
    /// <returns></returns>
    public static new CommandResponse<TView, TError> Fail(TError errorCode)
    {
        return new CommandResponse<TView, TError>(errorCode);
    }

    /// <summary>
    ///     构造一个代表执行成功的 <see cref="CommandResponse{TView,TError}" />。
    /// </summary>
    /// <returns>代表执行成功的 <see cref="CommandResponse{TView,TError}" />。</returns>
    public static new CommandResponse<TView, TError> Success()
    {
        return new CommandResponse<TView, TError>();
    }

    /// <summary>
    ///     构造一个代表执行成功的 <see cref="CommandResponse{TView,TError}" />。
    /// </summary>
    /// <param name="view">执行结果。</param>
    /// <returns></returns>
    public static CommandResponse<TView, TError> Success(TView view)
    {
        return new CommandResponse<TView, TError>(view);
    }

    /// <inheritdoc />
    public object? GetResult()
    {
        return Response;
    }
}