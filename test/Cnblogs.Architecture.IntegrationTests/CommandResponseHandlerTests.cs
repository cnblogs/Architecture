using System.Net.Http.Json;
using Cnblogs.Architecture.IntegrationTestProject;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cnblogs.Architecture.IntegrationTests;

public class CommandResponseHandlerTests
{
    [Fact]
    public async Task HandleCommandResponse_HavingError_BadRequestAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync("/api/v1/strings/1", new UpdatePayload(true));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Should().HaveClientError();
        content.Should().Be(TestError.Default.Name);
    }

    [Fact]
    public async Task HandleCommandResponse_Success_OkAsync()
    {
        // Arrange
        var builder = new WebApplicationFactory<Program>();

        // Act
        var response = await builder.CreateClient().PutAsJsonAsync("/api/v1/strings/1", new UpdatePayload(false));

        // Assert
        response.Should().BeSuccessful();
    }
}
