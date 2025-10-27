using System.Net;
using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class CustomModelBinderTests
{
    [Fact]
    public async Task PagingParamsModelBinder_Normal_NotNullAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient()
            .GetFromJsonAsync<PagingParams>("/api/v1/mvc/paging?pageIndex=1&pageSize=30");

        // Assert
        Assert.Equivalent(new PagingParams(1, 30), response);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?pageIndex=1")]
    [InlineData("?pageSize=1")]
    public async Task PagingParamsModelBinder_NoPageIndexOrPageSize_NullAsync(string query)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync($"/api/v1/mvc/paging{query}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("hello")]
    public async Task PagingParamsModelBinder_PageIndexInvalid_FailAsync(string pageIndex)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync($"/api/v1/mvc/paging?pageIndex={pageIndex}&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("-1")]
    public async Task PagingParamsModelBinder_PageSizeInvalid_FailAsync(string pageSize)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync($"/api/v1/mvc/paging?pageIndex=1&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
