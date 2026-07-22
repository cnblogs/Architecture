using System.Diagnostics;
using Cnblogs.Architecture.Tool;
using Cnblogs.Architecture.Tool.Generation;

// cnb-arch-tool (dotnet cnb): tooling for Cnblogs.Architecture.
// Usage: dotnet cnb serviceagent generate --api-project <path> --output <dir> --namespace <ns> [--clean]
if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
{
    PrintUsage();
    return 0;
}

if (args[0] != "serviceagent")
{
    Console.Error.WriteLine($"Unknown module '{args[0]}'. Available modules: serviceagent.");
    PrintUsage();
    return 2;
}

if (args.Length < 2 || args[1] == "--help" || args[1] == "-h")
{
    PrintUsage();
    return 0;
}

if (args[1] != "generate")
{
    Console.Error.WriteLine($"Unknown serviceagent command '{args[1]}'. Available commands: generate.");
    PrintUsage();
    return 2;
}

var options = ParseOptions(args.Skip(2).ToArray());
if (options is null)
{
    return 2;
}

return await RunGenerateAsync(options);

static void PrintUsage()
{
    Console.WriteLine(
        """
        cnb-arch-tool (dotnet cnb) — tooling for Cnblogs.Architecture.

        Usage: dotnet cnb <module> <command> [options]

        Modules:
          serviceagent   Generate strongly-typed CQRS service agents.

        serviceagent generate:
          dotnet cnb serviceagent generate --api-project <api-csproj-or-dir> --output <client-dir> --namespace <ns> [--clean]

        Requires the API project to reference the Cnblogs.Architecture.ServiceAgent.Design package.
        """);
}

static GenerateOptions? ParseOptions(string[] args)
{
    string? apiProject = null;
    string? output = null;
    var ns = "Generated.ServiceAgents";
    var clean = false;
    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--api-project":
                apiProject = Next(args, ref i);
                break;
            case "--output":
                output = Next(args, ref i);
                break;
            case "--namespace":
                ns = Next(args, ref i) ?? "Generated.ServiceAgents";
                break;
            case "--clean":
                clean = true;
                break;
            default:
                Console.Error.WriteLine($"Unknown option '{args[i]}'.");
                return null;
        }
    }

    if (string.IsNullOrEmpty(apiProject) || string.IsNullOrEmpty(output))
    {
        Console.Error.WriteLine("--api-project and --output are required.");
        return null;
    }

    return new GenerateOptions(apiProject, output, ns, clean);
}

static string? Next(string[] args, ref int i)
{
    return i + 1 < args.Length ? args[++i] : null;
}

static async Task<int> RunGenerateAsync(GenerateOptions options)
{
    var apiProjectPath = ResolveProjectPath(options.ApiProject);
    if (apiProjectPath is null)
    {
        Console.Error.WriteLine($"Could not find an API project at '{options.ApiProject}'.");
        return 3;
    }

    Console.WriteLine($"Building {apiProjectPath} ...");
    if (await RunDotnetAsync(["build", apiProjectPath, "-v", "quiet"]) != 0)
    {
        Console.Error.WriteLine("Building the API project failed; the manifest cannot be exported.");
        return 4;
    }

    var manifestPath = Path.Combine(Path.GetTempPath(), "cnblogs-sa-manifest-" + Guid.NewGuid().ToString("N") + ".json");
    Directory.CreateDirectory(options.Output);
    var runEnv = new Dictionary<string, string?>
    {
        ["CNBLOGS_SA_EXPORT"] = manifestPath,
        // Activate the Design package's hosting startup so the exporter registers itself.
        ["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "Cnblogs.Architecture.ServiceAgent.Design",
        // Bind an ephemeral port so the design-time run never fights another server for the default port.
        ["ASPNETCORE_URLS"] = "http://127.0.0.1:0"
    };

    Console.WriteLine($"Exporting endpoints from {apiProjectPath} ...");
    var exitCode = await RunDotnetAsync(["run", "--project", apiProjectPath, "--no-build", "--no-launch-profile"], runEnv);
    if (exitCode != 0)
    {
        Console.Error.WriteLine($"Running the API project failed (exit code {exitCode}). Check the output above.");
        return 5;
    }

    if (!File.Exists(manifestPath))
    {
        Console.Error.WriteLine("The API project did not write an endpoint manifest. Ensure it references the Cnblogs.Architecture.ServiceAgent.Design package.");
        return 6;
    }

    var manifest = ManifestReader.Read(manifestPath);
    Console.WriteLine($"Read manifest: {manifest.Groups.Count} group(s).");

    if (options.Clean)
    {
        CleanGeneratedFiles(options.Output);
    }

    var emitter = new ServiceAgentEmitter();
    var files = emitter.Emit(manifest, options.Namespace);
    foreach (var diagnostic in emitter.Diagnostics)
    {
        Console.WriteLine("warning: " + diagnostic);
    }

    foreach (var file in files)
    {
        var target = Path.Combine(options.Output, file.FileName);
        await File.WriteAllTextAsync(target, file.Content);
        Console.WriteLine($"Generated {target}");
    }

    if (File.Exists(manifestPath))
    {
        File.Delete(manifestPath);
    }

    Console.WriteLine("Done.");
    return 0;
}

static string? ResolveProjectPath(string input)
{
    if (Directory.Exists(input))
    {
        var csproj = Directory.GetFiles(input, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
        return csproj;
    }

    return File.Exists(input) ? Path.GetFullPath(input) : null;
}

static void CleanGeneratedFiles(string output)
{
    if (!Directory.Exists(output))
    {
        return;
    }

    foreach (var file in Directory.EnumerateFiles(output, "*.cs"))
    {
        var content = File.ReadAllText(file);
        if (content.Contains("generated by cnb-arch-tool", StringComparison.Ordinal))
        {
            File.Delete(file);
            Console.WriteLine($"Removed stale generated file {file}");
        }
    }
}

static async Task<int> RunDotnetAsync(string[] arguments, IReadOnlyDictionary<string, string?>? extraEnv = null)
{
    var info = new ProcessStartInfo("dotnet", arguments)
    {
        UseShellExecute = false,
        RedirectStandardOutput = false,
        RedirectStandardError = false
    };

    if (extraEnv is not null)
    {
        foreach (var (key, value) in extraEnv)
        {
            info.Environment[key] = value;
        }
    }

    var process = Process.Start(info);
    if (process is null)
    {
        Console.Error.WriteLine("Could not start 'dotnet'. Is the .NET SDK on PATH?");
        return -1;
    }

    await process.WaitForExitAsync();
    return process.ExitCode;
}
