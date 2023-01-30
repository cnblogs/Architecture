using System.Text.Json;

using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using FluentAssertions;

using Mapster;

namespace Cnblogs.Architecture.UnitTests.Infrastructure;

public class PagedListTests
{
    [Fact]
    public void PagedListSelfMappable()
    {
        // Arrange
        PagedList<ADto> pagedList = new(new List<ADto> { new("1") });

        // Act
        var mapped = pagedList.Adapt<PagedList<BDto>>();

        // Assert
        pagedList.Items.Select(a => a.Value).Should().BeEquivalentTo(mapped.Items.Select(b => b.Value));
    }

    [Fact]
    public void PagedListSerializableToAndFromJson()
    {
        // Arrange
        PagedList<string> pagedList = new(new[] { "a", "b", "c" });

        // Act
        var json = JsonSerializer.Serialize(pagedList);
        var deserialized = JsonSerializer.Deserialize<PagedList<string>>(json);

        // Assert
        pagedList.Should().BeEquivalentTo(deserialized);
    }

    private record ADto(string Value);

    private record BDto(string Value);
}