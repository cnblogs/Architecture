using System.Net;
using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
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
        Assert.True(response.IsSuccessStatusCode);
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task GetItem_NotFondAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync("/api/v1/strings/1?found=false");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(content);
        Assert.NotEmpty(content.Items);
    }

    [Fact]
    public async Task CreateItems_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync("/api/v1/strings", new { NeedError = false });

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task UpdateItem_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync("/api/v1/strings/1", new { NeedError = false });

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DeleteItem_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().DeleteAsync("/api/v1/strings/1?needError=false");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GetItem_NullableRouteValue_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var responses = new List<HttpResponseMessage>
        {
            await builder.CreateClient().GetAsync("/api/v1/apps/-/strings/-/value"),
            await builder.CreateClient().GetAsync("/api/v1/apps/-/strings/1/value"),
            await builder.CreateClient().GetAsync("/api/v1/apps/someApp/strings/-/value"),
            await builder.CreateClient().GetAsync("/api/v1/apps/someApp/strings/1/value")
        };

        // Assert
        Assert.All(responses, r => Assert.True(r.IsSuccessStatusCode));
    }

    [Fact]
    public async Task GetItem_MapHeadAndGet_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var uris = new[]
        {
            "/api/v1/apps/-/strings/-/value", "/api/v1/apps/-/strings/1/value", "/api/v1/apps/someApp/strings/-/value", "/api/v1/apps/someApp/strings/1/value"
        }.Select(x => new HttpRequestMessage(HttpMethod.Head, x));
        var responses = new List<HttpResponseMessage>();
        foreach (var uri in uris)
        {
            responses.Add(await builder.CreateClient().SendAsync(uri, HttpCompletionOption.ResponseHeadersRead));
        }

        // Assert
        Assert.All(responses, r => Assert.True(r.IsSuccessStatusCode));
    }

    [Fact]
    public async Task PostItem_GenericMap_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PostAsJsonAsync(
            "/api/v1/generic-map/strings",
            new CreateCommand(false, "data"));

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PutItem_GenericMap_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/generic-map/strings",
            new UpdateCommand(1, false, false));

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DeleteCommand_GenericMap_SuccessAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var queryBuilder = new QueryStringBuilder().Add("needError", false);
        var response = await builder.CreateClient().DeleteAsync("/api/v1/generic-map/strings/1" + queryBuilder.Build());

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}
