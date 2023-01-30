using MediatR;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
/// 集成事件。
/// </summary>
/// <param name="Id">集成事件的 Id。</param>
/// <param name="CreatedTime">集成事件创建时间。</param>
public record IntegrationEvent(Guid Id, DateTimeOffset CreatedTime) : INotification
{
    /// <summary>
    ///     跟踪 Id。
    /// </summary>
    public Guid? TraceId { get; set; }
}
