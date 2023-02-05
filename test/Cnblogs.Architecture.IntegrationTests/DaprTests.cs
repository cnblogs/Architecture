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
    public async Task Dapr_subscribe_endpoint_is_ok()
    {
        var response = await _httpClient.GetAsync("/dapr/subscribe");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Contain("pubsub");
    }
}
