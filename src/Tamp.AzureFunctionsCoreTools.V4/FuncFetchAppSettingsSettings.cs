namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>
/// Settings for <c>func azure functionapp fetch-app-settings &lt;name&gt;</c>.
/// Pulls cloud App Settings into the project's <c>local.settings.json</c>
/// — handy for "sync prod settings to local dev" workflows.
/// </summary>
public sealed class FuncFetchAppSettingsSettings : FuncSettingsBase
{
    public string? AppName { get; set; }
    public string? Slot { get; set; }
    public string? Subscription { get; set; }
    public Secret? AccessToken { get; set; }
    public string? ManagementUrl { get; set; }

    public FuncFetchAppSettingsSettings SetAppName(string name) { AppName = name; return this; }
    public FuncFetchAppSettingsSettings SetSlot(string slot) { Slot = slot; return this; }
    public FuncFetchAppSettingsSettings SetSubscription(string nameOrId) { Subscription = nameOrId; return this; }
    public FuncFetchAppSettingsSettings SetAccessToken(Secret? token) { AccessToken = token; return this; }
    public FuncFetchAppSettingsSettings SetManagementUrl(string url) { ManagementUrl = url; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(AppName))
            throw new InvalidOperationException("func azure functionapp fetch-app-settings: AppName is required.");

        var args = new List<string> { "azure", "functionapp", "fetch-app-settings", AppName! };
        EmitGlobalArguments(args);
        if (!string.IsNullOrEmpty(Slot)) { args.Add("--slot"); args.Add(Slot!); }
        if (!string.IsNullOrEmpty(Subscription)) { args.Add("--subscription"); args.Add(Subscription!); }
        if (!string.IsNullOrEmpty(ManagementUrl)) { args.Add("--management-url"); args.Add(ManagementUrl!); }
        if (AccessToken is not null) args.Add("--access-token-stdin");
        return args;
    }

    protected override string? BuildStandardInput() => AccessToken?.Reveal();
    protected override IReadOnlyList<Secret> CollectSecrets() =>
        AccessToken is null ? Array.Empty<Secret>() : new[] { AccessToken };
}
