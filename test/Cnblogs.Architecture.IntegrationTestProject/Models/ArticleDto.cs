using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.IntegrationTestProject.Models;

public class ArticleDto : IModel, IEnrichableModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool Enriched { get; set; }

    /// <inheritdoc />
    public bool EnrichedAfter { get; set; }
}
