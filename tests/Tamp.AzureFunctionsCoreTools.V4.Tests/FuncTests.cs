using System.IO;
using Tamp;
using Xunit;

namespace Tamp.AzureFunctionsCoreTools.V4.Tests;

public sealed class FuncTests
{
    private static Tool FakeTool(string name = "func") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    // ---- shape ----

    [Fact]
    public void Every_Verb_Uses_Tool_Path()
    {
        var t = FakeTool();
        Assert.Equal(t.Executable.Value, Func.Publish(t, s => s.SetAppName("x")).Executable);
        Assert.Equal(t.Executable.Value, Func.LogStream(t, s => s.SetAppName("x")).Executable);
        Assert.Equal(t.Executable.Value, Func.FetchAppSettings(t, s => s.SetAppName("x")).Executable);
        Assert.Equal(t.Executable.Value, Func.ListFunctions(t, s => s.SetAppName("x")).Executable);
        Assert.Equal(t.Executable.Value, Func.Version(t).Executable);
        Assert.Equal(t.Executable.Value, Func.Raw(t, "--help").Executable);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("logstream")]
    [InlineData("fetch-app-settings")]
    [InlineData("list-functions")]
    public void Azure_FunctionApp_Verbs_Start_With_Three_Tokens(string action)
    {
        var plan = action switch
        {
            "publish" => Func.Publish(FakeTool(), s => s.SetAppName("x")),
            "logstream" => Func.LogStream(FakeTool(), s => s.SetAppName("x")),
            "fetch-app-settings" => Func.FetchAppSettings(FakeTool(), s => s.SetAppName("x")),
            "list-functions" => Func.ListFunctions(FakeTool(), s => s.SetAppName("x")),
            _ => throw new InvalidOperationException(),
        };
        Assert.Equal(["azure", "functionapp", action], plan.Arguments.Take(3));
        Assert.Equal("x", plan.Arguments[3]);
    }

    [Fact]
    public void Global_Args_ScriptRoot_And_Verbose_Round_Trip()
    {
        var plan = Func.Publish(FakeTool(), s => s
            .SetAppName("x")
            .SetScriptRoot("./src")
            .SetVerbose());
        Assert.Contains("--script-root", plan.Arguments);
        Assert.Contains("./src", plan.Arguments);
        Assert.Contains("--verbose", plan.Arguments);
    }

    // ---- publish ----

    [Fact]
    public void Publish_Requires_AppName()
    {
        Assert.Throws<InvalidOperationException>(() => Func.Publish(FakeTool(), s => { }));
    }

    [Fact]
    public void Publish_Minimal_App_Name_Only()
    {
        var plan = Func.Publish(FakeTool(), s => s.SetAppName("my-func-app"));
        Assert.Equal(["azure", "functionapp", "publish", "my-func-app"], plan.Arguments);
    }

    [Theory]
    [InlineData(FuncBuildMode.Default, false, null)]
    [InlineData(FuncBuildMode.Remote, true, "remote")]
    [InlineData(FuncBuildMode.Local, true, "local")]
    [InlineData(FuncBuildMode.NoBuild, false, null)]
    public void Publish_BuildMode_Round_Trips(FuncBuildMode mode, bool expectBuildFlag, string? expectBuildValue)
    {
        var plan = Func.Publish(FakeTool(), s => s.SetAppName("x").SetBuildMode(mode));
        if (mode == FuncBuildMode.NoBuild)
        {
            Assert.Contains("--no-build", plan.Arguments);
            Assert.DoesNotContain("--build", plan.Arguments);
        }
        else if (mode == FuncBuildMode.Default)
        {
            Assert.DoesNotContain("--build", plan.Arguments);
            Assert.DoesNotContain("--no-build", plan.Arguments);
        }
        else
        {
            Assert.Equal(expectBuildFlag, plan.Arguments.Contains("--build"));
            Assert.Contains(expectBuildValue!, plan.Arguments);
        }
    }

    [Fact]
    public void Publish_All_Flags_Round_Trip()
    {
        var plan = Func.Publish(FakeTool(), s => s
            .SetAppName("strata-api-test")
            .SetSlot("staging")
            .SetSubscription("sub-id")
            .SetBuildMode(FuncBuildMode.Remote)
            .SetForce()
            .SetNoZip()
            .SetBuildNativeDeps()
            .SetAdditionalPackages("python3-dev libevent-dev")
            .SetShowKeys()
            .SetPublishLocalSettings()
            .SetOverwriteSettings()
            .SetCsx()
            .SetDotnetCliParams("--no-incremental")
            .SetDotnetVersion("8.0")
            .SetManagementUrl("https://management.usgovcloudapi.net"));
        var args = plan.Arguments;
        Assert.Contains("--slot", args); Assert.Contains("staging", args);
        Assert.Contains("--subscription", args); Assert.Contains("sub-id", args);
        Assert.Contains("--build", args); Assert.Contains("remote", args);
        Assert.Contains("--force", args);
        Assert.Contains("--nozip", args);
        Assert.Contains("--build-native-deps", args);
        Assert.Contains("--additional-packages", args); Assert.Contains("python3-dev libevent-dev", args);
        Assert.Contains("--show-keys", args);
        Assert.Contains("--publish-local-settings", args);
        Assert.Contains("--overwrite-settings", args);
        Assert.Contains("--csx", args);
        Assert.Contains("--dotnet-cli-params", args); Assert.Contains("--no-incremental", args);
        Assert.Contains("--dotnet-version", args); Assert.Contains("8.0", args);
        Assert.Contains("--management-url", args); Assert.Contains("https://management.usgovcloudapi.net", args);
    }

    [Fact]
    public void Publish_PublishSettingsOnly_Round_Trips()
    {
        var plan = Func.Publish(FakeTool(), s => s.SetAppName("x").SetPublishSettingsOnly());
        Assert.Contains("--publish-settings-only", plan.Arguments);
    }

    // ---- access token routing ----

    [Fact]
    public void Publish_Access_Token_Flows_Through_Stdin_Not_Argv()
    {
        var token = new Secret("Azure OAuth", "ya29.fake.access.token.1234");
        var plan = Func.Publish(FakeTool(), s => s
            .SetAppName("x")
            .SetAccessToken(token));

        // --access-token-stdin IS in argv (just the flag, no value)...
        Assert.Contains("--access-token-stdin", plan.Arguments);
        // ...the value goes through stdin...
        Assert.Equal("ya29.fake.access.token.1234", plan.StandardInput);
        // ...the token value MUST NOT appear in argv...
        Assert.DoesNotContain("ya29.fake.access.token.1234", plan.Arguments);
        Assert.DoesNotContain("--access-token", plan.Arguments.Where(a => a == "--access-token"));
        // ...and the Secret joins the redaction table.
        Assert.Single(plan.Secrets);
        Assert.Same(token, plan.Secrets[0]);
    }

    [Fact]
    public void Publish_Without_Access_Token_Has_Empty_Secrets_And_No_Stdin()
    {
        var plan = Func.Publish(FakeTool(), s => s.SetAppName("x"));
        Assert.Null(plan.StandardInput);
        Assert.Empty(plan.Secrets);
        Assert.DoesNotContain("--access-token-stdin", plan.Arguments);
    }

    [Theory]
    [InlineData("LogStream")]
    [InlineData("FetchAppSettings")]
    [InlineData("ListFunctions")]
    public void Verbs_Also_Route_Access_Token_Through_Stdin(string verb)
    {
        var token = new Secret("Azure OAuth", "shared-token-value");
        var plan = verb switch
        {
            "LogStream" => Func.LogStream(FakeTool(), s => s.SetAppName("x").SetAccessToken(token)),
            "FetchAppSettings" => Func.FetchAppSettings(FakeTool(), s => s.SetAppName("x").SetAccessToken(token)),
            "ListFunctions" => Func.ListFunctions(FakeTool(), s => s.SetAppName("x").SetAccessToken(token)),
            _ => throw new InvalidOperationException(),
        };
        Assert.Equal("shared-token-value", plan.StandardInput);
        Assert.DoesNotContain("shared-token-value", plan.Arguments);
        Assert.Contains("--access-token-stdin", plan.Arguments);
        Assert.Single(plan.Secrets);
    }

    // ---- logstream ----

    [Fact]
    public void LogStream_Requires_AppName()
    {
        Assert.Throws<InvalidOperationException>(() => Func.LogStream(FakeTool(), s => { }));
    }

    [Fact]
    public void LogStream_Browser_Flag()
    {
        var plan = Func.LogStream(FakeTool(), s => s.SetAppName("x").SetBrowser());
        Assert.Contains("--browser", plan.Arguments);
    }

    // ---- fetch-app-settings ----

    [Fact]
    public void FetchAppSettings_Requires_AppName()
    {
        Assert.Throws<InvalidOperationException>(() => Func.FetchAppSettings(FakeTool(), s => { }));
    }

    [Fact]
    public void FetchAppSettings_With_Slot()
    {
        var plan = Func.FetchAppSettings(FakeTool(), s => s
            .SetAppName("strata-api-prod")
            .SetSlot("staging"));
        Assert.Equal("azure", plan.Arguments[0]);
        Assert.Equal("functionapp", plan.Arguments[1]);
        Assert.Equal("fetch-app-settings", plan.Arguments[2]);
        Assert.Equal("strata-api-prod", plan.Arguments[3]);
        Assert.Contains("--slot", plan.Arguments);
        Assert.Contains("staging", plan.Arguments);
    }

    // ---- list-functions ----

    [Fact]
    public void ListFunctions_Requires_AppName()
    {
        Assert.Throws<InvalidOperationException>(() => Func.ListFunctions(FakeTool(), s => { }));
    }

    [Fact]
    public void ListFunctions_ShowKeys_Flag()
    {
        var plan = Func.ListFunctions(FakeTool(), s => s.SetAppName("x").SetShowKeys());
        Assert.Contains("--show-keys", plan.Arguments);
    }

    // ---- version ----

    [Fact]
    public void Version_Is_Just_The_Flag()
    {
        Assert.Equal(["--version"], Func.Version(FakeTool()).Arguments);
    }

    // ---- raw ----

    [Fact]
    public void Raw_Requires_Args()
    {
        Assert.Throws<ArgumentException>(() => Func.Raw(FakeTool()));
    }

    [Fact]
    public void Raw_Forwards_Verbatim()
    {
        var plan = Func.Raw(FakeTool(), "extensions", "install", "--package", "Microsoft.Azure.WebJobs.Extensions.SignalRService");
        Assert.Equal(
            ["extensions", "install", "--package", "Microsoft.Azure.WebJobs.Extensions.SignalRService"],
            plan.Arguments);
    }

    // ---- nulls ----

    [Fact]
    public void Null_Tool_Throws_For_Every_Verb()
    {
        Assert.Throws<ArgumentNullException>(() => Func.Publish(null!, s => s.SetAppName("x")));
        Assert.Throws<ArgumentNullException>(() => Func.LogStream(null!, s => s.SetAppName("x")));
        Assert.Throws<ArgumentNullException>(() => Func.FetchAppSettings(null!, s => s.SetAppName("x")));
        Assert.Throws<ArgumentNullException>(() => Func.ListFunctions(null!, s => s.SetAppName("x")));
        Assert.Throws<ArgumentNullException>(() => Func.Version(null!));
        Assert.Throws<ArgumentNullException>(() => Func.Raw(null!, "--help"));
    }

    [Fact]
    public void Null_Configurer_Throws_For_Required_Verbs()
    {
        Assert.Throws<ArgumentNullException>(() => Func.Publish(FakeTool(), (Action<FuncPublishSettings>)null!));
        Assert.Throws<ArgumentNullException>(() => Func.LogStream(FakeTool(), (Action<FuncLogStreamSettings>)null!));
        Assert.Throws<ArgumentNullException>(() => Func.FetchAppSettings(FakeTool(), (Action<FuncFetchAppSettingsSettings>)null!));
        Assert.Throws<ArgumentNullException>(() => Func.ListFunctions(FakeTool(), (Action<FuncListFunctionsSettings>)null!));
    }

    [Fact]
    public void Working_Directory_And_Env_Flow_To_Plan()
    {
        var cwd = Path.GetTempPath();
        var plan = Func.Publish(FakeTool(), s => s
            .SetAppName("x")
            .SetWorkingDirectory(cwd)
            .SetEnv("AZURE_CORE_OUTPUT", "none"));
        Assert.Equal(cwd, plan.WorkingDirectory);
        Assert.Equal("none", plan.Environment["AZURE_CORE_OUTPUT"]);
    }
}
