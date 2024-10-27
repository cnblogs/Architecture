using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Cnblogs.Architecture.IntegrationTestProject;
using Cnblogs.Architecture.IntegrationTestProject.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class CustomJsonConverterTests
{
    private static readonly JsonSerializerOptions WebDefaults = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData("/api/v1/mvc/json/long-to-string/")]
    [InlineData("/api/v1/long-to-string/")]
    public async Task LongToJson_WriteLongToString_CanBeParsedByServerAsync(string baseUrl)
    {
        // Arrange
        const long id = 202410267558024668;
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync(baseUrl + id);
        var serverObject = await response.Content.ReadFromJsonAsync<LongToStringModel>(WebDefaults);

        // Assert
        serverObject.Should().BeEquivalentTo(new LongToStringModel() { Id = id });
    }

    [Theory]
    [InlineData("/api/v1/mvc/json/long-to-string/")]
    [InlineData("/api/v1/long-to-string/")]
    public async Task LongToJson_WriteLongToString_IsStringInJsonAsync(string baseUrl)
    {
        // Arrange
        const long id = 202410267558024668;
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync(baseUrl + id);
        var browserObject = await response.Content.ReadFromJsonAsync<JsonElement>(WebDefaults);

        // Assert
        browserObject.EnumerateObject().First().Value.GetString().Should().Be(id.ToString());
    }

    [Theory]
    [InlineData("/api/v1/mvc/json/long-to-string/")]
    [InlineData("/api/v1/long-to-string/")]
    public async Task LongToJson_ReadLongFromString_SuccessAsync(string url)
    {
        // Arrange
        const string json = """
                            {
                                "id": "202410267558024668"
                            }
                            """;

        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json"));
        var model = await response.Content.ReadFromJsonAsync<JsonElement>(WebDefaults);

        // Assert
        model.EnumerateObject().First().Value.GetString().Should().Be("202410267558024668");
    }

    [Theory]
    [InlineData("/api/v1/mvc/json/long-to-string/")]
    [InlineData("/api/v1/long-to-string/")]
    public async Task LongToJson_ReadLongFromNumber_SuccessAsync(string url)
    {
        // Arrange
        const string json = """
                            {
                                "id": 202410267558024668
                            }
                            """;

        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json"));
        var model = await response.Content.ReadFromJsonAsync<JsonElement>(WebDefaults);

        // Assert
        model.EnumerateObject().First().Value.GetString().Should().Be("202410267558024668");
    }
}
