namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// 验证错误。
/// </summary>
/// <param name="Message">错误信息。</param>
/// <param name="ParameterName">参数名称。</param>
public record ValidationError(string Message, string? ParameterName);