# CODEX_CONTEXT_SNAPSHOT

Generated: 2026-05-16 10:26:30 +08:00

## Repo State

* Branch: `main`
* HEAD: `95c9eec`
* Working tree entries after noise filtering: 137
* Noise filters applied: `.vs`, build outputs, test outputs, generated session/result artifacts, local UI/Codex output folders

## Read First

1. `AGENTS.md`
2. `CODEX_PLAN.md`
3. `CODEX_CONTEXT.md`
4. `CODEX_CONTEXT_SNAPSHOT.md`
5. `CHANGELOG.md`

## Solution Projects

* `src\ATS.Application\ATS.Application.csproj`
* `src\ATS.Cli\ATS.Cli.csproj`
* `src\ATS.Core\ATS.Core.csproj`
* `src\ATS.Ui\ATS.Ui.csproj`
* `tests\ATS.Tests\ATS.Tests.csproj`

## Latest Changelog Entries

* ## [2026-05-16 10:25]
* ## [2026-05-16 11:20]
* ## [2026-04-13 21:45]
* ## [2026-04-12 09:31]
* ## [2026-04-12 08:56]

## Filtered Working Tree Summary

### docs (17)

* `[??] docs/architecture/ui-design-constraints.md`
* `[??] docs/architecture/variable-system.md`
* `[M] docs/README.md`
* `[M] docs/user-manual/index.md`
* `[??] docs/user-manual/user-manual/cli/ai-analyze.md`
* `[??] docs/user-manual/user-manual/cli/ai-compare.md`
* `[??] docs/user-manual/user-manual/cli/ai-regress.md`
* `[??] docs/user-manual/user-manual/cli/ai-render.md`
* `[M] docs/user-manual/user-manual/cli/device-exec.md`
* `[M] docs/user-manual/user-manual/cli/overview.md`
* `[M] docs/user-manual/user-manual/cli/recipe-validate.md`
* `[M] docs/user-manual/user-manual/cli/script-run.md`
* `... +5 more`

### repo-root (8)

* `[M] .gitignore`
* `[M] AGENTS.md`
* `[M] AutoTestSystem.Next.sln`
* `[M] CHANGELOG.md`
* `[??] CODEX_CONTEXT.md`
* `[??] CODEX_CONTEXT_SNAPSHOT.md`
* `[M] CODEX_PLAN.md`
* `[M] README.md`

### samples (4)

* `[??] samples/recipes/all-spec-types.recipe.json`
* `[??] samples/recipes/all-spec-types-fail.recipe.json`
* `[??] samples/recipes/variable-system.recipe.json`
* `[??] samples/specs/all-spec-types.spec.json`

### src/ATS.Application (52)

* `[??] src/ATS.Application/Ai/AiAnalysisBundleBuilder.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisBundleComparison.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisBundleComparisonBuilder.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisBundleWriter.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisComparisonHtmlRenderer.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisComparisonHtmlWriter.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisHtmlRenderer.cs`
* `[??] src/ATS.Application/Ai/AiAnalysisHtmlWriter.cs`
* `[??] src/ATS.Application/Ai/AiRegressionChecker.cs`
* `[??] src/ATS.Application/Ai/AiRegressionHtmlRenderer.cs`
* `[??] src/ATS.Application/Ai/AiRegressionHtmlWriter.cs`
* `[??] src/ATS.Application/Ai/ArtifactSummaryBuilder.cs`
* `... +40 more`

### src/ATS.Cli (2)

* `[M] src/ATS.Cli/ATS.Cli.csproj`
* `[M] src/ATS.Cli/Program.cs`

### src/ATS.Core (44)

* `[M] src/ATS.Core/ATS.Core.csproj`
* `[??] src/ATS.Core/Devices/IDeviceFactory.cs`
* `[??] src/ATS.Core/Devices/IDeviceSession.cs`
* `[??] src/ATS.Core/Models/AiAnalysisBundle.cs`
* `[??] src/ATS.Core/Models/AiEvidenceItem.cs`
* `[??] src/ATS.Core/Models/AiObservation.cs`
* `[??] src/ATS.Core/Models/AiProviderRequest.cs`
* `[??] src/ATS.Core/Models/AiProviderResponse.cs`
* `[??] src/ATS.Core/Models/AiRegressionCheckResult.cs`
* `[??] src/ATS.Core/Models/AiRegressionFinding.cs`
* `[??] src/ATS.Core/Models/AiRegressionStatus.cs`
* `[??] src/ATS.Core/Models/AiRunAnalysisRequest.cs`
* `... +32 more`

### src/ATS.Ui (7)

* `[??] src/ATS.Ui/ATS.Ui.csproj`
* `[??] src/ATS.Ui/MainForm.cs`
* `[??] src/ATS.Ui/Models/LoadedArtifacts.cs`
* `[??] src/ATS.Ui/Program.cs`
* `[??] src/ATS.Ui/Services/ArtifactLoader.cs`
* `[??] src/ATS.Ui/Services/CliCommandRunner.cs`
* `[??] src/ATS.Ui/Services/WorkspaceLocator.cs`

### tests/ATS.Tests (2)

* `[M] tests/ATS.Tests/ATS.Tests.csproj`
* `[M] tests/ATS.Tests/Program.cs`

### tools (1)

* `[??] tools/update-codex-context.ps1`

## Notes

* This snapshot is for fast review only. Open source files only after the snapshot shows the area that matters.
* If this file is stale, regenerate it with `powershell -ExecutionPolicy Bypass -File tools/update-codex-context.ps1`.
