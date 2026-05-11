# Tamp.AzureFunctionsCoreTools

Wrapper for **Azure Functions Core Tools** (the `func` CLI). Built
for CI deploys, especially **Flex Consumption (FC1)** where ADO's
`AzureFunctionApp@2` task doesn't fit.

```csharp
using Tamp.AzureFunctionsCoreTools.V4;
```

| Package | func | Status |
|---|---|---|
| `Tamp.AzureFunctionsCoreTools.V4` | 4.x | preview |

Requires `Tamp.Core ≥ 1.0.5`.

## Verbs (v0.1.0)

| Verb | Maps to | Notes |
|---|---|---|
| `Publish` | `func azure functionapp publish <app>` | Primary CI verb. FC1-friendly: `SetBuildMode(Remote\|Local\|NoBuild)`, `--slot`, `--show-keys`, `--publish-local-settings`. Access token typed as `Secret`, routed via stdin. |
| `LogStream` | `func azure functionapp logstream <app>` | Long-running tail; not a CI gate verb. |
| `FetchAppSettings` | `func azure functionapp fetch-app-settings <app>` | Sync cloud App Settings to local.settings.json. |
| `ListFunctions` | `func azure functionapp list-functions <app>` | Verification step post-deploy. |
| `Version` | `func --version` | |
| `Raw` | escape hatch | For init / new / host start / settings / extensions / templates / durable / kubernetes. |

**Global flags (every verb)**: `--script-root`, `--verbose`.

## Quick example — Strata's FC1 deploy

```csharp
using Tamp;
using Tamp.AzureFunctionsCoreTools.V4;

[NuGetPackage("func", UseSystemPath = true)]
readonly Tool FuncTool = null!;

[Secret("Azure OAuth access token", EnvironmentVariable = "AZURE_ACCESS_TOKEN")]
readonly Secret AzureToken = null!;

Target PublishFunctions => _ => _.Executes(() =>
    Func.Publish(FuncTool, s => s
        .SetAppName($"strata-api-{Env}")
        .SetBuildMode(FuncBuildMode.Remote)           // FC1 server-side build
        .SetDotnetVersion("8.0")
        .SetShowKeys()
        .SetAccessToken(AzureToken)                   // via stdin, not argv
        .SetWorkingDirectory(RootDirectory / "src" / "Strata.Functions")));
```

## CI behaviour to know about

**Access token via stdin, never argv.** The Functions Core Tools
support `--access-token-stdin` for exactly this reason. The wrapper
routes any `SetAccessToken(Secret)` through stdin automatically — the
token value never appears in argv or process listings, and the
`Secret` joins the runner's redaction table so subsequent log lines
that echo it get scrubbed.

You typically get the token via `az account get-access-token`:

```csharp
// One target captures the az token, the next pipes it into func.
Target FetchAzureToken => _ => _.Executes(() => /* parse az output */);
Target PublishFunctions => _ => _.DependsOn(nameof(FetchAzureToken)).Executes(...);
```

Or store it in an env-var Secret and let Tamp's redaction handle it.

**FC1 (Flex Consumption) specifics.** Strata's pipeline uses
`SetBuildMode(FuncBuildMode.Remote)` — FC1 expects the server-side
build path. Local builds (`SetBuildMode(FuncBuildMode.Local)`) and
no-build (`SetBuildMode(FuncBuildMode.NoBuild)`) are also available
for non-FC1 plans.

**`--publish-local-settings` is dangerous.** Pushes your
local.settings.json contents to the cloud as App Settings. Off by
default. Almost always wrong in CI — you typically want App Settings
managed via Bicep / ARM (see `Tamp.Bicep`).

## What's NOT in v0.1.0

Available via `Func.Raw(...)`:
- `func init` / `func new` — scaffolding (interactive)
- `func host start` — local dev runtime (long-running, non-CI)
- `func settings add/list/decrypt/encrypt` — local.settings.json management
- `func extensions install` — Functions extensions
- `func templates list`
- `func durable ...` — Durable Functions management
- `func kubernetes ...` — k8s deploy
- `func azurecontainerapps ...` — ACA deploy

Slated for v0.2.0+ if there's demand.

## Releasing

See [MAINTAINERS.md](MAINTAINERS.md).
