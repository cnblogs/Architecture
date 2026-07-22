using System.Text.Json;
using System.Text.Json.Serialization;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.ServiceAgent.Design;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.UnitTests.Cqrs;

[Collection(SerialCollection.Name)]
public class ServiceAgentExporterEndToEndTests
{
    public record SingleQuery(string? AppId = null, int? StringId = null, bool Found = true)
        : IQuery<string>;

    public record CreateCommand : ICommand<string, TestError>
    {
        public bool ValidateOnly => false;
    }

    public class TestError : Enumeration
    {
        public static readonly TestError None = new(0, nameof(None));

        public TestError(int id, string name)
            : base(id, name)
        {
        }
    }

    [Fact]
    public async Task AddServiceAgentExporter_WritesManifestWhenEnvVarSetAsync()
    {
        // Arrange — env var is process-global; we set it before building the app and clear it in finally.
        var previous = Environment.GetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable);
        var path = Path.Combine(Path.GetTempPath(), "cnblogs-sa-e2e-" + Guid.NewGuid().ToString("N") + ".json");
        Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, path);

        WebApplication? app = null;
        try
        {
            var builder = WebApplication.CreateBuilder();
            // Bind an ephemeral port so this WebApplication does not fight other test classes for the default port.
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddServiceAgentExporter();
            app = builder.Build();

            app.MapQuery<SingleQuery>("apps/{appId}/strings/{stringId:int}/value");
            app.MapPostCommand<CreateCommand>("items");

            // Act — StartAsync fires ApplicationStarted, which triggers the hosted service. The hosted service
            // writes the manifest and then calls StopApplication, so the host should stop on its own.
            await app.StartAsync();

            // Wait for the manifest to appear on disk (the exporter writes synchronously inside the
            // ApplicationStarted callback, so the file is usually already there; poll as a safety net).
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (!File.Exists(path) && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50);
            }

            // Assert — the file must exist.
            Assert.True(
                File.Exists(path),
                $"Service-agent manifest was not written within the deadline. Expected path: {path}. The export pipeline (env-var gate + ApplicationStarted hook + writer) did not run.");

            var json = await File.ReadAllTextAsync(path);
            var manifest = JsonSerializer.Deserialize<EndpointManifest>(
                json,
                new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } })!;

            Assert.Equal(1, manifest.SchemaVersion);
            Assert.NotEmpty(manifest.Groups);

            var endpoints = manifest.Groups.SelectMany(g => g.Endpoints).ToList();
            Assert.NotEmpty(endpoints);
            Assert.Contains(endpoints, e => e.RequestTypeName == nameof(CreateCommand));
        }
        finally
        {
            // Stop the app if it's still running (it usually stops itself inside the exporter).
            if (app is not null)
            {
                try
                {
                    await app.StopAsync();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed by the host shutdown path — fine.
                }
            }

            // Restore the env var to its previous state.
            Environment.SetEnvironmentVariable(
                ServiceAgentGeneration.ExportPathEnvironmentVariable,
                previous);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task HostingStartup_WritesManifestWhenActivatedViaEnvVarsAsync()
    {
        // Arrange — activates the production path: the [assembly: HostingStartup] in the Design assembly fires when
        // ASPNETCORE_HOSTINGSTARTUPASSEMBLIES lists it. Deliberately NOT calling AddServiceAgentExporter.
        var previousExport = Environment.GetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable);
        var previousHosting = Environment.GetEnvironmentVariable(ServiceAgentGeneration.HostingStartupEnvironmentVariable);
        var path = Path.Combine(Path.GetTempPath(), "cnblogs-sa-hosting-" + Guid.NewGuid().ToString("N") + ".json");
        Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, path);
        Environment.SetEnvironmentVariable(
            ServiceAgentGeneration.HostingStartupEnvironmentVariable,
            ServiceAgentGeneration.AssemblyName);

        WebApplication? app = null;
        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            app = builder.Build();
            app.MapQuery<SingleQuery>("apps/{appId}/strings/{stringId:int}/value");
            app.MapPostCommand<CreateCommand>("items");

            // Act
            await app.StartAsync();
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (!File.Exists(path) && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50);
            }

            // Assert — the hosting-startup path (the one the dotnet-cnblogs-sa tool relies on) must register the
            // hosted service and write the manifest.
            Assert.True(
                File.Exists(path),
                "Hosting-startup activation did not write the manifest; the [assembly: HostingStartup] path is broken.");
            var json = await File.ReadAllTextAsync(path);
            var manifest = JsonSerializer.Deserialize<EndpointManifest>(
                json,
                new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } })!;
            Assert.Equal(1, manifest.SchemaVersion);
            Assert.NotEmpty(manifest.Groups);
        }
        finally
        {
            if (app is not null)
            {
                try
                {
                    await app.StopAsync();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, previousExport);
            Environment.SetEnvironmentVariable(
                ServiceAgentGeneration.HostingStartupEnvironmentVariable,
                previousHosting);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task NoEnvVar_HostStaysUpAndNoManifestWrittenAsync()
    {
        // Arrange — the hosted service is registered but the export env var is NOT set, so it must be a no-op:
        // no manifest written and the host must not stop itself.
        var previous = Environment.GetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable);
        var path = Path.Combine(Path.GetTempPath(), "cnblogs-sa-neg-" + Guid.NewGuid().ToString("N") + ".json");
        Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, null);

        WebApplication? app = null;
        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddServiceAgentExporter();
            app = builder.Build();
            app.MapQuery<SingleQuery>("apps/{appId}/strings/{stringId:int}/value");

            // Act
            await app.StartAsync();
            await Task.Delay(300);

            // Assert
            Assert.False(File.Exists(path), "A manifest was written even though the export env var was not set.");
            Assert.False(
                app.Lifetime.ApplicationStopping.IsCancellationRequested,
                "The host stopped itself without the export env var being set.");
        }
        finally
        {
            if (app is not null)
            {
                try
                {
                    await app.StopAsync();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, previous);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void DoubleRegistration_RegistersSingleHostedService()
    {
        // Arrange / Act — registering via both the explicit extension (twice) is idempotent.
        var services = new ServiceCollection();
        services.AddServiceAgentExporter();
        services.AddServiceAgentExporter();

        // Assert
        var count = services.Count(d => d.ImplementationType == typeof(ServiceAgentExporterHostedService));
        Assert.Equal(1, count);
    }
}
