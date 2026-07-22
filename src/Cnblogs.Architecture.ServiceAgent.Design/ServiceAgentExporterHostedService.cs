using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cnblogs.Architecture.ServiceAgent.Design;

/// <summary>
///     On application start, when a service-agent generation run is active (<see cref="ServiceAgentGeneration" />),
///     exports the registered CQRS endpoints as a manifest and then stops the host. Otherwise it is a no-op, so it is
///     safe to register unconditionally.
/// </summary>
public sealed class ServiceAgentExporterHostedService : IHostedService
{
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ServiceAgentExporterHostedService> _logger;

    /// <summary>Creates the exporter.</summary>
    /// <param name="endpointDataSource">The endpoint data source to enumerate on application start.</param>
    /// <param name="lifetime">The host application lifetime, used to hook <c>ApplicationStarted</c> and stop the host after the export.</param>
    /// <param name="logger">The logger.</param>
    public ServiceAgentExporterHostedService(
        EndpointDataSource endpointDataSource,
        IHostApplicationLifetime lifetime,
        ILogger<ServiceAgentExporterHostedService> logger)
    {
        _endpointDataSource = endpointDataSource;
        _lifetime = lifetime;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!ServiceAgentGeneration.IsActive)
        {
            return Task.CompletedTask;
        }

        _lifetime.ApplicationStarted.Register(ExportAndStop);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void ExportAndStop()
    {
        var path = ServiceAgentGeneration.ExportPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            _logger.LogInformation(
                "Service-agent generation run active; exporting endpoint manifest to {Path}",
                path);

            var manifest = EndpointManifestBuilder.Build(_endpointDataSource);
            EndpointManifestWriter.Write(path, manifest);

            _logger.LogInformation(
                "Service-agent manifest exported: {GroupCount} group(s), {EndpointCount} endpoint(s)",
                manifest.Groups.Count,
                manifest.Groups.Sum(g => g.Endpoints.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service-agent manifest export failed");
        }
        finally
        {
            // The generation run has done its job; stop the host so `dotnet run` exits cleanly.
            _lifetime.StopApplication();
        }
    }
}
