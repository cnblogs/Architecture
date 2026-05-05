using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Models;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class EnricherTests
{
    [Fact]
    public async Task SingleQuery_RequestEnrichment_EnrichedAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient()
            .GetFromJsonAsync<ArticleDto>("/api/v1/articles/1");

        // Assert
        Assert.True(response?.Enriched);
    }

    [Fact]
    public async Task PagedQuery_RequestEnrichment_EnrichedAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient()
            .GetFromJsonAsync<PagedList<ArticleDto>>("/api/v1/articles");

        // Assert
        Assert.NotNull(response);
        Assert.All(response.Items, a => Assert.True(a.Enriched));
    }

    [Fact]
    public async Task CommandResponse_RequestEnrichment_EnrichedAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/articles",
            new CreateArticlePayload("测试标题"));
        var commandResponse = await response.Content.ReadFromJsonAsync<ArticleDto>();

        // Assert
        Assert.True(commandResponse?.Enriched);
    }
}
