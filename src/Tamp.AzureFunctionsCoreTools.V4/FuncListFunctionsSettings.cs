namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>Settings for <c>func azure functionapp list-functions &lt;name&gt;</c> — list deployed functions in the app.</summary>
public sealed class FuncListFunctionsSettings : FuncSettingsBase
{
    public string? AppName { get; set; }
    public string? Slot { get; set; }
    public string? Subscription { get; set; }
    /// <summary>Print function keys alongside the function URLs. Maps to <c>--show-keys</c>.</summary>
    public bool ShowKeys { get; set; }
    public Secret? AccessToken { get; set; }
    public string? ManagementUrl { get; set; }

    public FuncListFunctionsSettings SetAppName(string name) { AppName = name; return this; }
    public FuncListFunctionsSettings SetSlot(string slot) { Slot = slot; return this; }
    public FuncListFunctionsSettings SetSubscription(string nameOrId) { Subscription = nameOrId; return this; }
    public FuncListFunctionsSettings SetShowKeys(bool v = true) { ShowKeys = v; return this; }
    public FuncListFunctionsSettings SetAccessToken(Secret? token) { AccessToken = token; return this; }
    public FuncListFunctionsSettings SetManagementUrl(string url) { ManagementUrl = url; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(AppName))
            throw new InvalidOperationException("func azure functionapp list-functions: AppName is required.");

        var args = new List<string> { "azure", "functionapp", "list-functions", AppName! };
        EmitGlobalArguments(args);
        if (!string.IsNullOrEmpty(Slot)) { args.Add("--slot"); args.Add(Slot!); }
        if (!string.IsNullOrEmpty(Subscription)) { args.Add("--subscription"); args.Add(Subscription!); }
        if (ShowKeys) args.Add("--show-keys");
        if (!string.IsNullOrEmpty(ManagementUrl)) { args.Add("--management-url"); args.Add(ManagementUrl!); }
        if (AccessToken is not null) args.Add("--access-token-stdin");
        return args;
    }

    protected override string? BuildStandardInput() => AccessToken?.Reveal();
    protected override IReadOnlyList<Secret> CollectSecrets() =>
        AccessToken is null ? Array.Empty<Secret>() : new[] { AccessToken };
}
