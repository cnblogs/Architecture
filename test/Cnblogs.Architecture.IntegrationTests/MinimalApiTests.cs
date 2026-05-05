using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class MinimalApiTests
{
    [Fact]
    public async Task Unicode_ResponseJsonWithChineseChars_RemainUnencodedAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync("/api/v1/articles");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("开发者", content);
    }

    [Fact]
    public async Task Pagination_FromRouteValues_SucceedAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();
        int pageIndex = 1, pageSize = 10;

        // Act
        var response = await builder.CreateClient().GetAsync($"/api/v1/articles/page:{pageIndex}-{pageSize}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains($"\"pageIndex\":{pageIndex}", content);
        Assert.Contains($"\"pageSize\":{pageSize}", content);
    }
}
