namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>
/// Build mode for <c>func azure functionapp publish</c>.
/// </summary>
public enum FuncBuildMode
{
    /// <summary>Default — let func decide based on the project shape.</summary>
    Default,
    /// <summary><c>--build remote</c> — Linux Consumption / Flex Consumption pattern. Server-side build.</summary>
    Remote,
    /// <summary><c>--build local</c> — local build then upload artifact.</summary>
    Local,
    /// <summary><c>--no-build</c> — pre-built artifact upload, no rebuild.</summary>
    NoBuild,
}

/// <summary>
/// Settings for <c>func azure functionapp publish &lt;name&gt;</c>.
/// Strata's primary verb (Flex Consumption deploys to FC1 Function
/// Apps). The access token is typed as <see cref="Secret"/> and
/// routed through <c>--access-token-stdin</c>, never argv.
/// </summary>
public sealed class FuncPublishSettings : FuncSettingsBase
{
    /// <summary>Required. Function app name (sometimes called "site name" in Azure docs).</summary>
    public string? AppName { get; set; }

    /// <summary>Deployment slot. Maps to <c>--slot</c>. Empty = production slot.</summary>
    public string? Slot { get; set; }

    /// <summary>Subscription override. Maps to <c>--subscription</c>.</summary>
    public string? Subscription { get; set; }

    /// <summary>Build mode. See <see cref="FuncBuildMode"/>.</summary>
    public FuncBuildMode BuildMode { get; set; } = FuncBuildMode.Default;

    /// <summary>Skip pre-publish checks. Maps to <c>--force</c>.</summary>
    public bool Force { get; set; }

    /// <summary>Run-from-package mode is on by default in func 4.x; setting this skips it. Maps to <c>--nozip</c>.</summary>
    public bool NoZip { get; set; }

    /// <summary>Build native deps (Python). Maps to <c>--build-native-deps</c>.</summary>
    public bool BuildNativeDeps { get; set; }

    /// <summary>Additional packages for native-deps build (Linux). Maps to <c>--additional-packages</c>.</summary>
    public string? AdditionalPackages { get; set; }

    /// <summary>Print function keys after publish. Maps to <c>--show-keys</c>.</summary>
    public bool ShowKeys { get; set; }

    /// <summary>Push local.settings.json to the cloud as App Settings. Maps to <c>--publish-local-settings</c>. <strong>Dangerous</strong> — typically off in CI.</summary>
    public bool PublishLocalSettings { get; set; }

    /// <summary>Only publish settings, no code. Maps to <c>--publish-settings-only</c>.</summary>
    public bool PublishSettingsOnly { get; set; }

    /// <summary>Overwrite existing cloud settings with local values. Maps to <c>--overwrite-settings</c>. Only meaningful with <see cref="PublishLocalSettings"/> or <see cref="PublishSettingsOnly"/>.</summary>
    public bool OverwriteSettings { get; set; }

    /// <summary>Old-style CSX dotnet functions. Maps to <c>--csx</c>.</summary>
    public bool Csx { get; set; }

    /// <summary>Extra args to inject into the internal <c>dotnet build</c> call. Maps to <c>--dotnet-cli-params</c>.</summary>
    public string? DotnetCliParams { get; set; }

    /// <summary>Pin .NET version for dotnet-isolated apps. Maps to <c>--dotnet-version</c>.</summary>
    public string? DotnetVersion { get; set; }

    /// <summary>Management URL override for sovereign clouds. Maps to <c>--management-url</c>.</summary>
    public string? ManagementUrl { get; set; }

    /// <summary>OAuth access token. Routed through <c>--access-token-stdin</c>, never argv. Joins the redaction table.</summary>
    public Secret? AccessToken { get; set; }

    public FuncPublishSettings SetAppName(string name) { AppName = name; return this; }
    public FuncPublishSettings SetSlot(string slot) { Slot = slot; return this; }
    public FuncPublishSettings SetSubscription(string nameOrId) { Subscription = nameOrId; return this; }
    public FuncPublishSettings SetBuildMode(FuncBuildMode mode) { BuildMode = mode; return this; }
    public FuncPublishSettings SetForce(bool v = true) { Force = v; return this; }
    public FuncPublishSettings SetNoZip(bool v = true) { NoZip = v; return this; }
    public FuncPublishSettings SetBuildNativeDeps(bool v = true) { BuildNativeDeps = v; return this; }
    public FuncPublishSettings SetAdditionalPackages(string packages) { AdditionalPackages = packages; return this; }
    public FuncPublishSettings SetShowKeys(bool v = true) { ShowKeys = v; return this; }
    public FuncPublishSettings SetPublishLocalSettings(bool v = true) { PublishLocalSettings = v; return this; }
    public FuncPublishSettings SetPublishSettingsOnly(bool v = true) { PublishSettingsOnly = v; return this; }
    public FuncPublishSettings SetOverwriteSettings(bool v = true) { OverwriteSettings = v; return this; }
    public FuncPublishSettings SetCsx(bool v = true) { Csx = v; return this; }
    public FuncPublishSettings SetDotnetCliParams(string args) { DotnetCliParams = args; return this; }
    public FuncPublishSettings SetDotnetVersion(string v) { DotnetVersion = v; return this; }
    public FuncPublishSettings SetManagementUrl(string url) { ManagementUrl = url; return this; }
    public FuncPublishSettings SetAccessToken(Secret? token) { AccessToken = token; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(AppName))
            throw new InvalidOperationException("func azure functionapp publish: AppName is required.");

        var args = new List<string> { "azure", "functionapp", "publish", AppName! };
        EmitGlobalArguments(args);
        if (!string.IsNullOrEmpty(Slot)) { args.Add("--slot"); args.Add(Slot!); }
        if (!string.IsNullOrEmpty(Subscription)) { args.Add("--subscription"); args.Add(Subscription!); }

        switch (BuildMode)
        {
            case FuncBuildMode.Remote: args.Add("--build"); args.Add("remote"); break;
            case FuncBuildMode.Local: args.Add("--build"); args.Add("local"); break;
            case FuncBuildMode.NoBuild: args.Add("--no-build"); break;
        }

        if (Force) args.Add("--force");
        if (NoZip) args.Add("--nozip");
        if (BuildNativeDeps) args.Add("--build-native-deps");
        if (!string.IsNullOrEmpty(AdditionalPackages)) { args.Add("--additional-packages"); args.Add(AdditionalPackages!); }
        if (ShowKeys) args.Add("--show-keys");
        if (PublishLocalSettings) args.Add("--publish-local-settings");
        if (PublishSettingsOnly) args.Add("--publish-settings-only");
        if (OverwriteSettings) args.Add("--overwrite-settings");
        if (Csx) args.Add("--csx");
        if (!string.IsNullOrEmpty(DotnetCliParams)) { args.Add("--dotnet-cli-params"); args.Add(DotnetCliParams!); }
        if (!string.IsNullOrEmpty(DotnetVersion)) { args.Add("--dotnet-version"); args.Add(DotnetVersion!); }
        if (!string.IsNullOrEmpty(ManagementUrl)) { args.Add("--management-url"); args.Add(ManagementUrl!); }
        if (AccessToken is not null) args.Add("--access-token-stdin");
        return args;
    }

    protected override string? BuildStandardInput() => AccessToken?.Reveal();

    protected override IReadOnlyList<Secret> CollectSecrets() =>
        AccessToken is null ? Array.Empty<Secret>() : new[] { AccessToken };
}
