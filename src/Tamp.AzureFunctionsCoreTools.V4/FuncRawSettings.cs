namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>Escape hatch for verbs we haven't typed: init, new, host start, settings, extensions, templates, durable, kubernetes.</summary>
public sealed class FuncRawSettings : FuncSettingsBase
{
    public List<string> RawArguments { get; } = [];

    public FuncRawSettings AddArgs(params string[] args) { RawArguments.AddRange(args); return this; }

    protected override IEnumerable<string> BuildVerbArguments() => RawArguments;
}
