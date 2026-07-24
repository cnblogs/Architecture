using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

namespace Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

public class CqrsToolDescriptorSchemaTests
{
    private static CqrsToolDescriptor Descriptor(Type type, RequestKind kind, CqrsAgentOptions? options = null)
    {
        return new CqrsToolDescriptor(type, kind, options ?? new CqrsAgentOptions(), new XmlDocumentationProvider());
    }

    private static HashSet<string> Properties(CqrsToolDescriptor descriptor)
    {
        return descriptor.JsonSchema.GetProperty("properties").EnumerateObject().Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> Required(CqrsToolDescriptor descriptor)
    {
        return descriptor.JsonSchema.TryGetProperty("required", out var required)
            ? required.EnumerateArray().Select(x => x.GetString()!).ToList()
            : [];
    }

    [Fact]
    public void Schema_CommandIncludesCtorParams_HidesValidateOnly()
    {
        var descriptor = Descriptor(typeof(AgentCreateCommand), RequestKind.Command);
        var properties = Properties(descriptor);

        Assert.Contains("title", properties);
        Assert.Contains("count", properties);
        Assert.DoesNotContain("validateOnly", properties);
        Assert.DoesNotContain("validateOnly", Required(descriptor));
    }

    [Fact]
    public void Schema_AllowValidateOnly_ExposesValidateOnly()
    {
        var descriptor = Descriptor(typeof(AgentValidateCommand), RequestKind.Command);
        Assert.Contains("validateOnly", Properties(descriptor));
    }

    [Fact]
    public void Schema_PageableQuery_FlattensPagingParams()
    {
        var descriptor = Descriptor(typeof(AgentPagedQuery), RequestKind.Query);
        var properties = Properties(descriptor);

        Assert.Contains("pageIndex", properties);
        Assert.Contains("pageSize", properties);
        Assert.DoesNotContain("pagingParams", properties);
        Assert.DoesNotContain("orderByString", properties);
    }

    [Fact]
    public void Schema_NestedRecord_HasPayloadProperty()
    {
        var descriptor = Descriptor(typeof(AgentCreateArticleCommand), RequestKind.Command);
        Assert.Contains("payload", Properties(descriptor));
    }

    [Fact]
    public void Name_DefaultsToRecordName()
    {
        var descriptor = Descriptor(typeof(AgentCreateCommand), RequestKind.Command);
        Assert.Equal("AgentCreateCommand", descriptor.Name);
    }

    [Fact]
    public void Description_TakenFromAttribute()
    {
        var descriptor = Descriptor(typeof(AgentCreateCommand), RequestKind.Command);
        Assert.Equal("Create an article.", descriptor.Description);
    }
}
