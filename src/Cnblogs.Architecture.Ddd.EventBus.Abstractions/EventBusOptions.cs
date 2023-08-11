using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.EventBus.Abstractions;

/// <summary>
///     Options for event bus.
/// </summary>
public class EventBusOptions
{
    /// <summary>
    ///     Interval for publish integration event. Defaults to 1000ms.
    /// </summary>
    public int Interval { get; set; } = 1000;

    /// <summary>
    ///     Maximum number of events that can be sent in one cycle. Pass <c>null</c> to disable limit. Defaults to <c>null</c>.
    /// </summary>
    public int? MaximumBatchSize { get; set; }

    /// <summary>
    ///     Maximum number of events that can be stored in buffer. An <see cref="EventBufferOverflowException"/> would be thrown when the number of events in buffer exceeds this limit. Pass <c>null</c> to disable limit. Defaults to <c>null</c>.
    /// </summary>
    public int? MaximumBufferSize { get; set; }

    /// <summary>
    ///     The maximum number of failure before downgrade. Defaults to 5.
    /// </summary>
    public int FailureCountBeforeDowngrade { get; set; } = 5;

    /// <summary>
    ///     Interval when downgraded. Defaults to 60000ms(1min).
    /// </summary>
    public int DowngradeInterval { get; set; } = 60 * 1000;

    /// <summary>
    ///     The maximum number of success before recover. Defaults to 1.
    /// </summary>
    public int SuccessCountBeforeRecover { get; set; } = 1;
}
