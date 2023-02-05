using System.Net;
using FluentAssertions;

namespace Cnblogs.Architecture.IntegrationTests;

[Collection(DddWebTestCollection.Name)]
public class DaprTests
{
    private readonly HttpClient _httpClient;

    public DaprTests(DddWebTestFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Dapr_SubscribeEndpoint_OkAsync()
    {
        // Act
        var response = await _httpClient.GetAsync("/dapr/subscribe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Contain("pubsub");
    }
}
