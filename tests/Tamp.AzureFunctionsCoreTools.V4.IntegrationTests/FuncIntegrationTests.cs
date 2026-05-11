using System.IO;
using Tamp;
using Xunit;
using Xunit.Abstractions;

namespace Tamp.AzureFunctionsCoreTools.V4.IntegrationTests;

/// <summary>
/// Exercises the wrapper against a real <c>func</c> binary. Avoids
/// verbs that require an authenticated Azure subscription — those
/// are consumer-pipeline territory. Sticks to <c>--version</c> and
/// <c>--help</c> shape probes.
/// </summary>
public sealed class FuncIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public FuncIntegrationTests(ITestOutputHelper output) => _output = output;

    private static string? ResolveOnPath(string baseName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var names = OperatingSystem.IsWindows()
            ? new[] { $"{baseName}.cmd", $"{baseName}.exe", $"{baseName}.bat", $"{baseName}.ps1", baseName }
            : new[] { baseName };
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(dir)) continue;
            foreach (var n in names)
            {
                var c = Path.Combine(dir, n);
                if (File.Exists(c)) return c;
            }
        }
        return null;
    }

    private static Tool ResolveTool() =>
        new(AbsolutePath.Create(ResolveOnPath("func")
            ?? throw new InvalidOperationException("func not found on PATH. Install: npm i -g azure-functions-core-tools@4")));

    private CaptureResult Run(CommandPlan plan)
    {
        _output.WriteLine($"$ {plan.Executable} {string.Join(' ', plan.Arguments)}");
        var result = ProcessRunner.Capture(plan);
        foreach (var line in result.Lines)
            _output.WriteLine($"  [{line.Type}] {line.Text}");
        _output.WriteLine($"  → exit {result.ExitCode}");
        return result;
    }

    [Fact]
    public void Version_Reports_4_x()
    {
        var tool = ResolveTool();
        var plan = Func.Version(tool);
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        Assert.Matches(@"^4\.\d+\.\d+", result.StdoutText.Trim());
    }

    [Fact]
    public void Raw_Help_Lists_Available_Contexts()
    {
        var tool = ResolveTool();
        var plan = Func.Raw(tool, "--help");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var context in new[] { "azure", "bundles", "durable", "extensions", "kubernetes", "settings", "templates" })
        {
            Assert.Contains(context, combined);
        }
    }

    [Fact]
    public void Raw_Publish_Help_Surfaces_Expected_Flags()
    {
        var tool = ResolveTool();
        var plan = Func.Raw(tool, "azure", "functionapp", "publish", "--help");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var flag in new[] { "--build", "--no-build", "--slot", "--access-token", "--access-token-stdin", "--show-keys", "--publish-local-settings", "--force" })
        {
            Assert.Contains(flag, combined);
        }
    }
}
