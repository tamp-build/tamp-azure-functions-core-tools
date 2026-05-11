namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>
/// Settings for <c>func azure functionapp logstream &lt;name&gt;</c>.
/// Long-running — typically used during deploy verification, not in a
/// CI gate. Set <see cref="Browser"/> to open the Live Stream Tab in a
/// browser instead of tailing to terminal.
/// </summary>
public sealed class FuncLogStreamSettings : FuncSettingsBase
{
    public string? AppName { get; set; }
    public string? Slot { get; set; }
    public string? Subscription { get; set; }
    /// <summary>Open the Live Stream web UI instead of tailing to terminal. Maps to <c>--browser</c>.</summary>
    public bool Browser { get; set; }
    public Secret? AccessToken { get; set; }
    public string? ManagementUrl { get; set; }

    public FuncLogStreamSettings SetAppName(string name) { AppName = name; return this; }
    public FuncLogStreamSettings SetSlot(string slot) { Slot = slot; return this; }
    public FuncLogStreamSettings SetSubscription(string nameOrId) { Subscription = nameOrId; return this; }
    public FuncLogStreamSettings SetBrowser(bool v = true) { Browser = v; return this; }
    public FuncLogStreamSettings SetAccessToken(Secret? token) { AccessToken = token; return this; }
    public FuncLogStreamSettings SetManagementUrl(string url) { ManagementUrl = url; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(AppName))
            throw new InvalidOperationException("func azure functionapp logstream: AppName is required.");

        var args = new List<string> { "azure", "functionapp", "logstream", AppName! };
        EmitGlobalArguments(args);
        if (!string.IsNullOrEmpty(Slot)) { args.Add("--slot"); args.Add(Slot!); }
        if (!string.IsNullOrEmpty(Subscription)) { args.Add("--subscription"); args.Add(Subscription!); }
        if (Browser) args.Add("--browser");
        if (!string.IsNullOrEmpty(ManagementUrl)) { args.Add("--management-url"); args.Add(ManagementUrl!); }
        if (AccessToken is not null) args.Add("--access-token-stdin");
        return args;
    }

    protected override string? BuildStandardInput() => AccessToken?.Reveal();
    protected override IReadOnlyList<Secret> CollectSecrets() =>
        AccessToken is null ? Array.Empty<Secret>() : new[] { AccessToken };
}
