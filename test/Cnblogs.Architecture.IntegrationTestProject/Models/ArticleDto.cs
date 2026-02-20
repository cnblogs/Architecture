using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Models;

public class ArticleDto : IModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
