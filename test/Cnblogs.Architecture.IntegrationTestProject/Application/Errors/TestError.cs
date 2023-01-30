using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Errors;

public class TestError : Enumeration
{
    public static readonly TestError Default = new TestError(1, "DefaultError");

    public TestError(int id, string name)
        : base(id, name)
    {
    }
}