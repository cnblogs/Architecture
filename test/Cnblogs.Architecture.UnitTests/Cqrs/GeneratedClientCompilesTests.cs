using System.Reflection;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.ServiceAgent.Design;
using Cnblogs.Architecture.Tool;
using Cnblogs.Architecture.Tool.Generation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedParameter.Local
namespace Cnblogs.Architecture.UnitTests.Cqrs;

[Collection(SerialCollection.Name)]
public class GeneratedClientCompilesTests
{
    [Fact]
    public async Task EmittedClient_CompilesWithNoRoslynErrorsAsync()
    {
        // Arrange — set the export env var BEFORE WebApplication.CreateBuilder so the exporter hosted service picks
        // it up on ApplicationStarted. Restore in finally (env var is process-global; serial collection prevents racing).
        var previous = Environment.GetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable);
        var path = Path.Combine(Path.GetTempPath(), "cnblogs-sa-compile-" + Guid.NewGuid().ToString("N") + ".json");
        Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, path);

        WebApplication? app = null;
        try
        {
            var builder = WebApplication.CreateBuilder();
            // Ephemeral port so this WebApplication does not race other test classes for the default port.
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddServiceAgentExporter();
            app = builder.Build();

            app.MapQuery<CqrsEndpointDescriptorBuilderTests.SingleQuery>("apps/{appId}/strings/{stringId:int}/value");
            app.MapPostCommand<CqrsEndpointDescriptorBuilderTests.CreateCommand>("items");
            app.MapDeleteCommand<CqrsEndpointDescriptorBuilderTests.DeleteCommand>("items/{id:int}");
            app.MapPostCommand<CqrsEndpointDescriptorBuilderTests.CreateBlogCommand>("blogs");
            app.MapPostCommand(
                "items/payload",
                (CqrsEndpointDescriptorBuilderTests.CreatePayload payload)
                    => new CqrsEndpointDescriptorBuilderTests.CreateCommand());
            app.MapPutCommand(
                "items/{id:int}/payload",
                (int id, CqrsEndpointDescriptorBuilderTests.UpdatePayload payload)
                    => new CqrsEndpointDescriptorBuilderTests.UpdateCommand());

            // Act — StartAsync fires ApplicationStarted, which triggers the exporter (writes the manifest and then
            // stops the host on its own).
            await app.StartAsync();
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (!File.Exists(path) && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50);
            }

            Assert.True(
                File.Exists(path),
                "Service-agent manifest was not written within the deadline. Expected path: " + path);

            // Read the manifest through the same reader the tool uses (Tool.Manifest.EndpointManifest).
            var manifest = ManifestReader.Read(path);

            // Emit the generated client source files into the test-compile namespace.
            var emitter = new ServiceAgentEmitter();
            var files = emitter.Emit(manifest, "TestCompile.Client");

            // Assert — the emitted file set must contain an interface, a class, and the extensions file.
            Assert.NotEmpty(files);
            Assert.Contains(
                files,
                f => f.FileName.StartsWith("I", StringComparison.Ordinal)
                     && f.FileName.EndsWith("Service.cs", StringComparison.Ordinal)
                     && !f.IsExtensionsFile);
            Assert.Contains(
                files,
                f => !f.FileName.StartsWith("I", StringComparison.Ordinal)
                     && f.FileName.EndsWith("Service.cs", StringComparison.Ordinal)
                     && !f.IsExtensionsFile);
            Assert.Contains(files, f => f.IsExtensionsFile);

            // At least one emitted class file must derive from CqrsServiceAgent<TestError> — the Test group's base
            // class. (TestError is a nested type, so the renderer emits it as CqrsEndpointDescriptorBuilderTests.TestError.)
            Assert.Contains(
                files,
                f => !f.IsExtensionsFile
                     && f.Content.Contains(
                         ": CqrsServiceAgent<CqrsEndpointDescriptorBuilderTests.TestError>(httpClient)",
                         StringComparison.Ordinal));

            // The command-as-body form (MapPostCommand<CreateBlogCommand>) generates a CreateBlogPayload POCO and uses
            // it as the body type; the command type itself is never referenced in any emitted file.
            Assert.Contains(files, f => f.FileName == "CreateBlogPayload.cs");
            Assert.Contains(
                files,
                f => !f.IsExtensionsFile
                     && f.Content.Contains("CreateBlogAsync(CreateBlogPayload payload)", StringComparison.Ordinal));
            Assert.DoesNotContain(
                files,
                f => !f.IsExtensionsFile
                     && f.Content.Contains(
                         "CqrsEndpointDescriptorBuilderTests.CreateBlogCommand",
                         StringComparison.Ordinal));

            // Compile EVERY emitted .cs file together via Roslyn and collect the diagnostics.
            var compilation = BuildCompilation(files);
            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            // Assert — no compilation errors. Report actual diagnostics on failure.
            Assert.True(
                errors.Count == 0,
                $"Generated client failed to compile. Errors ({errors.Count}):\n{string.Join("\n", errors.Select(d => $"  [{d.Id}] {d.GetMessage()}"))}");
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
                    // Already disposed by the host shutdown path — fine.
                }
            }

            Environment.SetEnvironmentVariable(ServiceAgentGeneration.ExportPathEnvironmentVariable, previous);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static CSharpCompilation BuildCompilation(List<EmittedFile> files)
    {
        var trees = files
            .Select(f => CSharpSyntaxTree.ParseText(f.Content, path: f.FileName))
            .ToList();

        // The generated source has no `using System.Net.Http;` directive; the manual end-to-end run only compiles
        // because the SDK injects <ImplicitUsings>enable</ImplicitUsings> and emits a GlobalUsings.g.cs. Replicate
        // that by adding a synthetic global-usings tree so the Roslyn compile matches what `dotnet build` does.
        var implicitUsings = """
                             global using System;
                             global using System.Collections.Generic;
                             global using System.IO;
                             global using System.Linq;
                             global using System.Net.Http;
                             global using System.Threading;
                             global using System.Threading.Tasks;
                             """;
        trees.Add(CSharpSyntaxTree.ParseText(implicitUsings, path: "GlobalUsings.g.cs"));

        var references = CollectMetadataReferences();

        return CSharpCompilation.Create(
            "TestCompile.Client",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithConcurrentBuild(true));
    }

    /// <summary>
    ///     Gather MetadataReferences for everything the generated client may touch: the test assembly itself (the
    ///     fixtures live there), every assembly already loaded into the AppDomain, and their transitive references.
    ///     We recursively walk <see cref="Assembly.GetReferencedAssemblies" /> because Roslyn resolves names lazily
    ///     and a type referenced only in generated code (e.g. QueryStringBuilder) may live in an assembly that has not
    ///     been loaded yet at the moment we collect.
    /// </summary>
    private static List<MetadataReference> CollectMetadataReferences()
    {
        // Touch these types so their assemblies (and their dependencies) are pulled into the AppDomain before the walk.
        _ = typeof(CqrsServiceAgent<>);
        _ = typeof(Enumeration);
        _ = typeof(IQuery<>);
        _ = typeof(PagedList<>);
        _ = typeof(QueryStringBuilder);
        _ = typeof(IServiceCollection);
        // Force System.Net.Http (HttpClient) to load — the generated class ctor takes an HttpClient parameter.
        _ = typeof(HttpClient);
        _ = typeof(Uri);
        _ = typeof(HttpMethod);

        var references = new Dictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<Assembly>();

        // Seed with everything currently loaded; the test assembly and its references will be in there.
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            queue.Enqueue(asm);
        }

        // Force-load the additional framework / package assemblies that the generated client references but that may
        // not have been pulled into the AppDomain yet (HttpClient lives in System.Net.Http; the versioning abstractions
        // and DI attributes are in Microsoft.Extensions.*). Use a name-based load so this does not depend on a path.
        foreach (var name in SeedAssemblyNames())
        {
            try
            {
                queue.Enqueue(Assembly.Load(name));
            }
            catch
            {
                // Not loadable in this host — skip; Roslyn will report an unresolved reference if it is actually needed.
            }
        }

        // Also seed with assemblies we know we need by typeof() — covers the case where Roslyn needs a ref that
        // nothing has caused to load yet (e.g. ServiceAgent has not been touched by the test fixture code path).
        foreach (var seed in SeedAssemblies())
        {
            queue.Enqueue(seed);
        }

        while (queue.Count > 0)
        {
            var asm = queue.Dequeue();
            if (asm.IsDynamic)
            {
                continue;
            }

            var location = asm.Location;
            if (string.IsNullOrEmpty(location))
            {
                continue;
            }

            if (!visited.Add(location))
            {
                continue;
            }

            references[location] = MetadataReference.CreateFromFile(location);

            foreach (var refName in asm.GetReferencedAssemblies())
            {
                if (visited.Contains(refName.FullName))
                {
                    continue;
                }

                Assembly? loaded;
                try
                {
                    loaded = Assembly.Load(refName);
                }
                catch
                {
                    // An assembly name in the reference graph may not be loadable in the test host (e.g. platform-
                    // specific stubs). Skip it — Roslyn will report an unresolved reference if it is actually needed.
                    continue;
                }

                queue.Enqueue(loaded);
            }
        }

        return references.Values.ToList();
    }

    private static IEnumerable<Assembly> SeedAssemblies()
    {
        // Force-load the assemblies that the generated client directly references. typeof() in C# does not by itself
        // guarantee the defining assembly is loaded before the JIT walks over the call site, but referencing an open
        // generic type's assembly here is enough to load it; we then return them so they seed the queue.
        return new[]
        {
            typeof(GeneratedClientCompilesTests).Assembly, typeof(CqrsServiceAgent).Assembly,
            typeof(Enumeration).Assembly, typeof(IQuery<>).Assembly, typeof(PagedList<>).Assembly,
            typeof(QueryStringBuilder).Assembly, typeof(IServiceCollection).Assembly, typeof(HttpClient).Assembly
        };
    }

    private static IEnumerable<string> SeedAssemblyNames()
    {
        // Force-load these so the reference-walk picks them up even when no test-fixture code path has loaded them.
        return
        [
            "System.Net.Http", "System.Net.Primitives", "System.Net.WebSockets", "System.Threading",
            "System.Text.Json", "Microsoft.Extensions.DependencyInjection.Abstractions",
            "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Compliance.Abstractions",
            "Microsoft.Extensions.Http.Resilience", "Microsoft.Extensions.Http.Logging",
            "Microsoft.Extensions.Options"
        ];
    }
}
