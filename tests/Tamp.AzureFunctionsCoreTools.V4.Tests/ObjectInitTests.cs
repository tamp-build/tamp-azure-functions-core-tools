using System.IO;
using Tamp;
using Xunit;

namespace Tamp.AzureFunctionsCoreTools.V4.Tests;

/// <summary>
/// Object-init overloads (TAM-161 satellite fanout). Every tool-bound wrapper with an
/// <c>Action&lt;TSettings&gt;</c> configurer also accepts a pre-populated settings instance;
/// both authoring styles must emit byte-identical <see cref="CommandPlan"/>s.
/// </summary>
public sealed class ObjectInitTests
{
    private static Tool FakeTool(string name = "func") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    [Fact]
    public void Publish_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();

        var fluent = Func.Publish(tool, s => s
            .SetAppName("strata-api-test")
            .SetSlot("staging")
            .SetBuildMode(FuncBuildMode.Remote)
            .SetForce()
            .SetDotnetVersion("8.0"));

        var objectInit = Func.Publish(tool, new FuncPublishSettings
        {
            AppName = "strata-api-test",
            Slot = "staging",
            BuildMode = FuncBuildMode.Remote,
            Force = true,
            DotnetVersion = "8.0",
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Every_Verb_ObjectInit_Surface_Compiles_And_Returns_CommandPlan()
    {
        // Smoke test: every Action<TSettings> wrapper accepts an object-init settings
        // instance and returns a non-null CommandPlan.
        var tool = FakeTool();
        Assert.NotNull(Func.Publish(tool, new FuncPublishSettings { AppName = "x" }));
        Assert.NotNull(Func.LogStream(tool, new FuncLogStreamSettings { AppName = "x" }));
        Assert.NotNull(Func.FetchAppSettings(tool, new FuncFetchAppSettingsSettings { AppName = "x" }));
        Assert.NotNull(Func.ListFunctions(tool, new FuncListFunctionsSettings { AppName = "x" }));
        Assert.NotNull(Func.Version(tool, new FuncVersionSettings()));
    }
}
