namespace Cnblogs.Architecture.IntegrationTestProject.Models;

public interface IEnrichableModel
{
    bool Enriched { get; set; }
    bool EnrichedAfter { get; set; }
}
