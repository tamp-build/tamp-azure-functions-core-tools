namespace Tamp.AzureFunctionsCoreTools.V4;

/// <summary>Facade for Azure Functions Core Tools (<c>func</c>) 4.x.</summary>
/// <remarks>
/// <para>Resolve via <c>[NuGetPackage(UseSystemPath = true)]</c>:</para>
/// <code>
/// [NuGetPackage("func", UseSystemPath = true)]
/// readonly Tool FuncTool;
/// </code>
/// </remarks>
public static class Func
{
    /// <summary><c>func azure functionapp publish &lt;app&gt;</c> — the canonical CI verb.</summary>
    public static CommandPlan Publish(Tool func, Action<FuncPublishSettings> configure)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new FuncPublishSettings();
        configure(s);
        return s.ToCommandPlan(func);
    }

    /// <summary><c>func azure functionapp logstream &lt;app&gt;</c> — long-running; typically used outside CI.</summary>
    public static CommandPlan LogStream(Tool func, Action<FuncLogStreamSettings> configure)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new FuncLogStreamSettings();
        configure(s);
        return s.ToCommandPlan(func);
    }

    /// <summary><c>func azure functionapp fetch-app-settings &lt;app&gt;</c> — sync cloud App Settings into local.settings.json.</summary>
    public static CommandPlan FetchAppSettings(Tool func, Action<FuncFetchAppSettingsSettings> configure)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new FuncFetchAppSettingsSettings();
        configure(s);
        return s.ToCommandPlan(func);
    }

    /// <summary><c>func azure functionapp list-functions &lt;app&gt;</c>.</summary>
    public static CommandPlan ListFunctions(Tool func, Action<FuncListFunctionsSettings> configure)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new FuncListFunctionsSettings();
        configure(s);
        return s.ToCommandPlan(func);
    }

    /// <summary><c>func --version</c>.</summary>
    public static CommandPlan Version(Tool func, Action<FuncVersionSettings>? configure = null)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        var s = new FuncVersionSettings();
        configure?.Invoke(s);
        return s.ToCommandPlan(func);
    }

    /// <summary>Escape hatch for verbs we haven't typed (init / new / host start / settings / extensions / templates / durable / kubernetes).</summary>
    public static CommandPlan Raw(Tool func, params string[] arguments)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        if (arguments is null || arguments.Length == 0)
            throw new ArgumentException("Raw requires at least one argument.", nameof(arguments));
        var s = new FuncRawSettings();
        s.AddArgs(arguments);
        return s.ToCommandPlan(func);
    }
}
