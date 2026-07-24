using System.Text.Json;

using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsToolDescriptorMarshalTests
{
    [Fact]
    public void MarshalResult_CommandSuccessWithoutView_ReturnsOk()
    {
        Assert.Equal("ok", CqrsToolDescriptor.MarshalResult(CommandResponse<AgentTestError>.Success()));
    }

    [Fact]
    public void MarshalResult_CommandSuccessWithView_ReturnsView()
    {
        var response = CommandResponse<string, AgentTestError>.Success("hello");

        Assert.Equal("hello", CqrsToolDescriptor.MarshalResult(response));
    }

    [Fact]
    public void MarshalResult_ValidationError_ReturnsValidationPayload()
    {
        var errors = new ValidationErrors { new("bad title", "Title") };
        var response = new CommandResponse<AgentTestError>
        {
            IsValidationError = true,
            ErrorMessage = "invalid",
            ValidationErrors = errors,
        };
        var json = Json(CqrsToolDescriptor.MarshalResult(response));

        Assert.Contains("\"error\":\"validation\"", json);
        Assert.Contains("\"Title\"", json);
        Assert.Contains("bad title", json);
    }

    [Fact]
    public void MarshalResult_ConcurrentError_ReturnsRetryablePayload()
    {
        var response = new CommandResponse<AgentTestError> { IsConcurrentError = true, LockAcquired = false };
        var json = Json(CqrsToolDescriptor.MarshalResult(response));

        Assert.Contains("\"error\":\"concurrent\"", json);
    }

    [Fact]
    public void MarshalResult_EnumError_ReturnsErrorCodePayload()
    {
        var response = CommandResponse<AgentTestError>.Fail(AgentTestError.Default);
        var json = Json(CqrsToolDescriptor.MarshalResult(response));

        Assert.Contains("\"error\":\"DefaultError\"", json);
    }

    [Fact]
    public void MarshalResult_NullQueryResponse_ReturnsNotFound()
    {
        Assert.Equal("not found", CqrsToolDescriptor.MarshalResult(null));
    }

    [Fact]
    public void MarshalResult_QueryView_ReturnsViewAsIs()
    {
        Assert.Equal("hello", CqrsToolDescriptor.MarshalResult("hello"));
    }

    private static string Json(object? value)
    {
        return JsonSerializer.Serialize(value);
    }
}
