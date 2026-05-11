namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>Settings for <c>func --version</c>.</summary>
public sealed class FuncVersionSettings : FuncSettingsBase
{
    protected override IEnumerable<string> BuildVerbArguments()
    {
        yield return "--version";
    }
}
