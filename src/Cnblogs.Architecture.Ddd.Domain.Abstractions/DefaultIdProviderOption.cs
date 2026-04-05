namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// Options for <see cref="DefaultIdProvider"/>.
/// </summary>
public class DefaultIdProviderOption
{
    /// <summary>
    /// Current id provider's instanceId, can be machineId, podId or containerId.
    /// </summary>
    public int InstanceId { get; set; }

    /// <summary>
    /// Strategy to take when sequence overflows. Default to throw.
    /// </summary>
    public SequenceOverflowStrategy WhenSequenceOverflow { get; set; }

    internal IdGen.SequenceOverflowStrategy IdGenStrategy
        => WhenSequenceOverflow switch
        {
            SequenceOverflowStrategy.Throw => IdGen.SequenceOverflowStrategy.Throw,
            SequenceOverflowStrategy.SpinWait => IdGen.SequenceOverflowStrategy.SpinWait,
            _ => IdGen.SequenceOverflowStrategy.Throw
        };
}
