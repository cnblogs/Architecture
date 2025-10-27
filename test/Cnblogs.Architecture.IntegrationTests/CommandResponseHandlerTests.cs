using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Application.Queries;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.IntegrationTests;

public class CommandResponseHandlerTests
{
    public static IEnumerable<object[]> ErrorPayloads { get; } = new List<object[]>
    {
        new object[] { true, false }, new object[] { false, true }
    };

    [Fact]
    public async Task MinimalApi_NoCqrsVersionHeader_RawResultAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();
        var client = builder.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/strings/1", new UpdatePayload());
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task MinimalApi_CqrsV2_CommandResponseAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();
        var client = builder.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
        client.DefaultRequestHeaders.AppendCurrentCqrsVersion();

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/strings/1", new UpdatePayload());
        var content = await response.Content.ReadFromJsonAsync<CommandResponse<string, TestError>>();

        // Assert
        Assert.True(response.Headers.CqrsVersion() > 1);
        Assert.NotNull(content);
        Assert.False(string.IsNullOrEmpty(content.Response));
    }

    [Fact]
    public async Task Mvc_NoCqrsVersionHeader_RawResultAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();
        var client = builder.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/mvc/strings/1", new UpdatePayload());
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task Mvc_CurrentCqrsVersion_CommandResponseAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();
        var client = builder.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
        client.DefaultRequestHeaders.AppendCurrentCqrsVersion();

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/mvc/strings/1", new UpdatePayload());
        var content = await response.Content.ReadFromJsonAsync<CommandResponse<string, TestError>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.True(response.Headers.CqrsVersion() > 1);
        Assert.False(string.IsNullOrEmpty(content!.Response));
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task MinimalApi_HavingError_BadRequestAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/strings/1",
            new UpdatePayload(needExecutionError, needValidationError));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task MinimalApi_Success_OkAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync("/api/v1/strings/1", new UpdatePayload());

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task MinimalApi_HavingError_ProblemDetailsAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>().WithWebHostBuilder(
            w => w.ConfigureTestServices(s => s.AddCqrs(typeof(GetStringQuery).Assembly).UseProblemDetails()));

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/strings/1",
            new UpdatePayload(needExecutionError, needValidationError));
        var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(content);
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task MinimalApi_HavingError_CommandResponseAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var client = builder.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
        var response = await client.PutAsJsonAsync(
            "/api/v1/strings/1",
            new UpdatePayload(needExecutionError, needValidationError));
        var commandResponse = await response.Content.ReadFromJsonAsync<CommandResponse<TestError>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(commandResponse);
        Assert.False(commandResponse.IsSuccess());
        Assert.Equal(needValidationError, commandResponse.IsValidationError);
        Assert.Equal(needExecutionError, commandResponse.ErrorCode != null);
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task MinimalApi_HavingError_CustomContentAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var error = new TestError(1, "testError");
        var builder = new WebApplicationFactory<Program>().WithWebHostBuilder(
            w => w.ConfigureTestServices(
                s => s.AddCqrs(typeof(GetStringQuery).Assembly)
                    .UseCustomCommandErrorResponseMapper((_, _) => Results.BadRequest(error))));

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/strings/1",
            new UpdatePayload(needValidationError, needExecutionError));
        var content = await response.Content.ReadFromJsonAsync<TestError>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equivalent(error, content);
    }

    [Fact]
    public async Task Mvc_Success_OkAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync("/api/v1/mvc/strings/1", new UpdatePayload());

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task Mvc_HavingError_TextAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/mvc/strings/1",
            new UpdatePayload(needValidationError, needExecutionError));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task Mvc_HavingError_ProblemDetailAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>().WithWebHostBuilder(
            w => w.ConfigureTestServices(s => s.AddCqrs(typeof(UpdateCommand).Assembly).UseProblemDetails()));

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/mvc/strings/1",
            new UpdatePayload(needValidationError, needExecutionError));
        var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(content);
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task Mvc_HavingError_CommandResponseAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var client = builder.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/cqrs"));
        var response = await client.PutAsJsonAsync(
            "/api/v1/mvc/strings/1",
            new UpdatePayload(needExecutionError, needValidationError));
        var content = await response.Content.ReadFromJsonAsync<CommandResponse<TestError>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(content);
        Assert.False(content.IsSuccess());
        Assert.Equal(needValidationError, content.IsValidationError);
        Assert.Equal(needExecutionError, content.ErrorCode != null);
    }

    [Theory]
    [MemberData(nameof(ErrorPayloads))]
    public async Task Mvc_HavingError_CustomContentAsync(bool needValidationError, bool needExecutionError)
    {
        // Arrange
        var error = TestError.Default;
        var builder = new WebApplicationFactory<Program>().WithWebHostBuilder(
            w => w.ConfigureTestServices(
                s => s.AddCqrs(typeof(UpdateCommand).Assembly)
                    .UseCustomCommandErrorResponseMapper((_, _) => Results.BadRequest(error))));

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync(
            "/api/v1/mvc/strings/1",
            new UpdatePayload(needValidationError, needExecutionError));
        var content = await response.Content.ReadFromJsonAsync<TestError>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equivalent(error, content);
    }
}
