using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Infrastructure;

public static class TestData
{
    public static readonly ArticleDto[] Articles = [new() { Id = 1, Title = "作为一个高中生开发者，我的所思所想" }];
}
