namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>
/// Common base for <c>func &lt;context&gt; &lt;action&gt;</c> settings.
/// Concrete classes layer verb-specific args on top.
/// </summary>
public abstract class FuncSettingsBase
{
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>Override the function-app root directory. Maps to <c>--script-root</c>.</summary>
    public string? ScriptRoot { get; set; }

    /// <summary>Verbose logging. Maps to <c>--verbose</c>.</summary>
    public bool Verbose { get; set; }

    protected abstract IEnumerable<string> BuildVerbArguments();

    protected virtual string? BuildStandardInput() => null;

    protected virtual IReadOnlyList<Secret> CollectSecrets() => Array.Empty<Secret>();

    protected void EmitGlobalArguments(List<string> args)
    {
        if (!string.IsNullOrEmpty(ScriptRoot)) { args.Add("--script-root"); args.Add(ScriptRoot!); }
        if (Verbose) args.Add("--verbose");
    }

    public CommandPlan ToCommandPlan(Tool tool)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = BuildVerbArguments().ToList(),
            Environment = new Dictionary<string, string>(EnvironmentVariables),
            WorkingDirectory = WorkingDirectory,
            StandardInput = BuildStandardInput(),
            Secrets = CollectSecrets(),
        };
    }
}

/// <summary>Generic fluent setters for the shared base.</summary>
public static class FuncSettingsBaseExtensions
{
    public static T SetWorkingDirectory<T>(this T s, string? cwd) where T : FuncSettingsBase { s.WorkingDirectory = cwd; return s; }
    public static T SetEnv<T>(this T s, string key, string value) where T : FuncSettingsBase { s.EnvironmentVariables[key] = value; return s; }
    public static T SetScriptRoot<T>(this T s, string path) where T : FuncSettingsBase { s.ScriptRoot = path; return s; }
    public static T SetVerbose<T>(this T s, bool v = true) where T : FuncSettingsBase { s.Verbose = v; return s; }
}
