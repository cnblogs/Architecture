using System.Net;

using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject;
using FluentAssertions;

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
            .GetFromJsonAsync<PagingParams>("/api/v1/paging?pageIndex=1&pageSize=30");

        // Assert
        response.Should().BeEquivalentTo(new PagingParams(1, 30));
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
        var response = await builder.CreateClient().GetAsync($"/api/v1/paging{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("-1")]
    public async Task PagingParamsModelBinder_PageIndexInvalid_FailAsync(string pageIndex)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync($"/api/v1/paging?pageIndex={pageIndex}&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("-1")]
    public async Task PagingParamsModelBinder_PageSizeInvalid_FailAsync(string pageSize)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().GetAsync($"/api/v1/paging?pageIndex=1&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}