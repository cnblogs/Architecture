using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject;
using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class CqrsRouteMapperTests
{
    [Fact]
    public async Task GetItem_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync("/api/v1/strings/1");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Should().BeSuccessful();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ListItems_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync("/api/v1/strings?pageIndex=1&pageSize=30");
        var content = await response.Content.ReadFromJsonAsync<PagedList<string>>();

        // Assert
        response.Should().BeSuccessful();
        content.Should().NotBeNull();
        content!.Items.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateItems_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync("/api/v1/strings", new { NeedError = false });

        // Assert
        response.Should().BeSuccessful();
    }

    [Fact]
    public async Task UpdateItem_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync("/api/v1/strings/1", new { NeedError = false });

        // Assert
        response.Should().BeSuccessful();
    }

    [Fact]
    public async Task DeleteItem_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().DeleteAsync("/api/v1/strings/1?needError=false");

        // Assert
        response.Should().BeSuccessful();
    }
}