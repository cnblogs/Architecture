using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Domain;

public class DefaultIdProviderTests
{
    [Fact]
    public void NextReadable_NoEigen_UseInstanceIdAsEigen()
    {
        // Arrange
        var provider = new DefaultIdProvider(GetStoppedDatetimeProvider(), new DefaultIdProviderOption()
        {
            InstanceId = 999
        });

        // Act
        var id = provider.NextReadable().ToString();

        // Assert
        Assert.Equal("999", id[^5..^2]);
    }

    [Fact]
    public void NextReadable_WithEigen_UseGivenEigen()
    {
        // Arrange
        var provider = new DefaultIdProvider(GetStoppedDatetimeProvider(), new DefaultIdProviderOption()
        {
            InstanceId = 999
        });

        // Act
        var id = provider.NextReadable(888).ToString();

        // Assert
        Assert.Equal("888", id[^5..^2]);
    }

    [Fact]
    public void NextReadable_Concurrent_UniqueIn100()
    {
        // Arrange
        var provider = new DefaultIdProvider(GetStoppedDatetimeProvider(), new DefaultIdProviderOption()
        {
            InstanceId = 999
        });

        // Act
        var distinctCount = Enumerable.Range(0, 100).Select(_ => provider.NextReadable()).Distinct().Count();

        // Assert
        Assert.Equal(100, distinctCount);
    }

    [Fact]
    public void NextReadable_SetThrowWhenOverflow_ThrowWhenAbove100()
    {
        // Arrange
        var provider = new DefaultIdProvider(GetStoppedDatetimeProvider(), new DefaultIdProviderOption()
        {
            InstanceId = 999,
            WhenSequenceOverflow = SequenceOverflowStrategy.Throw
        });

        // Act
        var act = () =>
        {
            var ids = Enumerable.Range(0, 120).Select(_ => provider.NextReadable()).Distinct().ToList();
            Assert.Equal(120, ids.Count);
        };

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void NextReadable_SpinWaitWhenOverflow_NotThrowWhenAbove100()
    {
        // Arrange
        var provider = new DefaultIdProvider(new DefaultDateTimeProvider(), new DefaultIdProviderOption()
        {
            InstanceId = 999,
            WhenSequenceOverflow = SequenceOverflowStrategy.SpinWait
        });

        // Act
        var ids = Enumerable.Range(0, 120).Select(_ => provider.NextReadable()).Distinct().Count();

        // Assert
        Assert.Equal(120, ids);
    }

    [Fact]
    public void NextNumeric_Concurrent_UniqueAndProgressive()
    {
        // Arrange
        var provider = new DefaultIdProvider(GetStoppedDatetimeProvider(), new DefaultIdProviderOption
        {
            InstanceId = 999
        });

        // Act
        var distinct = Enumerable.Range(0, 120).Select(_ => provider.NextNumeric()).Distinct().ToList();
        var ordered = distinct.OrderBy(x => x).ToList();
        var distinctCount = distinct.Count;

        // Assert
        Assert.Equal(120, distinctCount);
        Assert.Equivalent(ordered, distinct);
    }

    private static IDateTimeProvider GetStoppedDatetimeProvider()
    {
        var timer = Substitute.For<IDateTimeProvider>();
        timer.Now().ReturnsForAnyArgs(DateTimeOffset.Now);
        return timer;
    }
}
