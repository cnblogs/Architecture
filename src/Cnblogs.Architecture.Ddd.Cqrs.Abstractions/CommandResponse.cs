using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Response returned by <see cref="ICommand{TError}"/>.
/// </summary>
public abstract record CommandResponse : IValidationResponse, ILockableResponse
{
    /// <summary>
    ///     Check if validation fails.
    /// </summary>
    public bool IsValidationError { get; init; }

    /// <summary>
    ///     Check if concurrent error happened.
    /// </summary>
    public bool IsConcurrentError { get; init; }

    /// <summary>
    ///     The error message returned by handler, return empty if no error or no error message.
    /// </summary>
    /// <remarks>
    ///     Do not rely on this property to determine if executed successful, use <see cref="IsSuccess"/> for this purpose.
    /// </remarks>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <inheritdoc />
    public ValidationErrors ValidationErrors { get; init; } = new();

    /// <inheritdoc />
    public bool LockAcquired { get; set; }

    /// <summary>
    ///     Check if command executed successfully.
    /// </summary>
    /// <returns>Return true if executed successfully, else return false.</returns>
    public virtual bool IsSuccess()
    {
        return IsValidationError == false && string.IsNullOrEmpty(ErrorMessage) && IsConcurrentError == false;
    }

    /// <summary>
    ///     Get error message.
    /// </summary>
    /// <returns>The error message, return <see cref="string.Empty"/> if no error.</returns>
    public virtual string GetErrorMessage() => ErrorMessage;
}

/// <summary>
///     Response returned by <see cref="ICommand{TError}"/>.
/// </summary>
/// <typeparam name="TError">The enumeration presenting errors.</typeparam>
public record CommandResponse<TError> : CommandResponse
    where TError : Enumeration
{
    /// <summary>
    ///     Create a successful <see cref="CommandResponse{TError}" />.
    /// </summary>
    public CommandResponse()
    {
        ErrorCode = default;
    }

    /// <summary>
    ///     Create a <see cref="CommandResponse{TError}" /> with given error.
    /// </summary>
    /// <param name="errorCode">The error.</param>
    public CommandResponse(TError errorCode)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorCode.Name;
    }

    /// <summary>
    ///     The error returned by handler, can be null if execution succeeded.
    /// </summary>
    public TError? ErrorCode { get; set; }

    /// <summary>
    ///     Create a failed <see cref="CommandResponse{TError}" /> with given error.
    /// </summary>
    /// <param name="errorCode">The error.</param>
    /// <returns>A failed <see cref="CommandResponse{TError}" /> with given error.</returns>
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
    ///     Create a successful <see cref="CommandResponse{TError}" />.
    /// </summary>
    /// <returns>A successful <see cref="CommandResponse{TError}" />.</returns>
    public static CommandResponse<TError> Success()
    {
        return new CommandResponse<TError>();
    }
}

/// <summary>
///     Response returned by <see cref="ICommand{TError}"/>.
/// </summary>
/// <typeparam name="TView">The model type been returned if execution completed without error.</typeparam>
/// <typeparam name="TError">The enumeration type representing errors.</typeparam>
public record CommandResponse<TView, TError> : CommandResponse<TError>, IObjectResponse
    where TError : Enumeration
{
    /// <summary>
    ///     Create a <see cref="CommandResponse{TView,TError}" />.
    /// </summary>
    public CommandResponse()
    {
    }

    /// <summary>
    ///     Create a <see cref="CommandResponse{TError}" /> with given error.
    /// </summary>
    /// <param name="errorCode">The error.</param>
    public CommandResponse(TError errorCode)
        : base(errorCode)
    {
    }

    /// <summary>
    ///     Create a <see cref="CommandResponse{TError}" /> with given model.
    /// </summary>
    /// <param name="response">The execution result.</param>
    private CommandResponse(TView response)
    {
        Response = response;
    }

    /// <summary>
    ///     The result been returned by command handler.
    /// </summary>
    /// <remarks>
    ///     This property can be null even if execution completed with no error.
    /// </remarks>
    public TView? Response { get; }

    /// <summary>
    ///     Create a <see cref="CommandResponse{TView,TError}" /> with given error.
    /// </summary>
    /// <param name="errorCode">The error.</param>
    /// <returns>A <see cref="CommandResponse{TView, TError}"/> with given error.</returns>
    public static new CommandResponse<TView, TError> Fail(TError errorCode)
    {
        return new CommandResponse<TView, TError>(errorCode);
    }

    /// <summary>
    ///     Create a <see cref="CommandResponse{TView,TError}" /> with no result nor error.
    /// </summary>
    /// <returns>The <see cref="CommandResponse{TView,TError}" />。</returns>
    public static new CommandResponse<TView, TError> Success()
    {
        return new CommandResponse<TView, TError>();
    }

    /// <summary>
    ///     Create a <see cref="CommandResponse{TView,TError}" /> with given result.
    /// </summary>
    /// <param name="view">The model to return.</param>
    /// <returns>A <see cref="CommandResponse{TView, TError}"/> with given result.</returns>
    public static CommandResponse<TView, TError> Success(TView? view)
    {
        return view is null ? Success() : new CommandResponse<TView, TError>(view);
    }

    /// <inheritdoc />
    public object? GetResult()
    {
        return Response;
    }
}