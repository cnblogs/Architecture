using Cnblogs.Architecture.IntegrationTestProject;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class MinimalApiTests
{
    [Fact]
    public async Task ResponseJsonWithChineseChars_RemainUnencodedAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync("/api/v1/articles");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine(content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("开发者", content);
    }
}
