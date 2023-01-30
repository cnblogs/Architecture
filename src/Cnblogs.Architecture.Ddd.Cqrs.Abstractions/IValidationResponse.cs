namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     <see cref="IValidatable" /> 的返回类型。
/// </summary>
public interface IValidationResponse
{
    /// <summary>
    ///     验证是否失败。
    /// </summary>
    bool IsValidationError { get; init; }

    /// <summary>
    ///     错误信息。
    /// </summary>
    string ErrorMessage { get; init; }

    /// <summary>
    /// 错误信息对象。
    /// </summary>
    ValidationError? ValidationError { get; init; }
}