# CHANGELOG

## [2026-05-16 10:25]

### Added

* Added explicit context-maintenance rules in `AGENTS.md` so every completed change must refresh the fast-review snapshot workflow

### Modified

* Tightened `CODEX_CONTEXT.md` update discipline so `CHANGELOG.md` is always the per-change history, `CODEX_CONTEXT_SNAPSHOT.md` is always refreshed after completed work, and `CODEX_CONTEXT.md` stays reserved for stable project-story changes

### Fixed

* Fixed the previous ambiguity around whether every edit should be written into both context files instead of using `CHANGELOG.md` for history and the snapshot file for current-state review

### Notes

* This is a workflow clarification change only; it does not alter CLI behavior or runtime features

## [2026-05-16 11:20]

### Added

* Added root-level `CODEX_CONTEXT.md` as a stable, human-maintained fast-review summary for future Codex sessions
* Added `tools/update-codex-context.ps1` to generate a filtered `CODEX_CONTEXT_SNAPSHOT.md` from branch, HEAD, solution, changelog, and noise-filtered working-tree state
* Added committed `CODEX_CONTEXT_SNAPSHOT.md` as the lightweight snapshot target that future sessions can read before scanning the full repository

### Modified

* Extended `AGENTS.md` with a required fast-review workflow so future work starts from project context files before broad repo inspection
* Expanded `.gitignore` to ignore local IDE and generated output folders that do not help product work and waste review time

### Fixed

* Fixed the previous onboarding inefficiency where new sessions had to spend extra tokens rediscovering branch state, roadmap context, and noisy working-tree changes from scratch

### Notes

* This change is intentionally internal and workflow-focused: it improves Codex review speed and context retention without changing CLI behavior or runtime execution

## [2026-04-13 21:45]

### Added

* Added thin Windows UI project `ATS.Ui` that reads existing run artifacts and invokes the existing ATS CLI instead of duplicating execution logic
* Added UI artifact loading support for `result.json`, `session.events.jsonl`, `session.log`, and session folders, including result tree, structured log grid, and session log viewer
* Added a minimal CLI runner panel in the UI for `test simulate`, `test run`, `script run`, `device exec`, `recipe validate`, `spec validate`, and custom ATS commands

### Modified

* Updated the solution and user manual to describe the new thin UI launch path and its CLI-first, artifact-driven scope

### Fixed

* Fixed the previous usability gap where users had to inspect artifacts and invoke CLI commands manually with no built-in thin UI shell for session/result/log review

### Notes

* `ATS.Ui` is intentionally thin: it consumes artifacts and triggers CLI commands, but it does not contain test business logic or replace the CLI as the source of truth

## [2026-04-12 09:31]

### Added

* Added mandatory roadmap requirements in `CODEX_PLAN.md` for station-grade multi-DUT execution, including per-DUT variable mapping, shared-resource locking, lock-event logging, and per-DUT data isolation

### Modified

* Updated the project plan so multi-DUT, channel-mapped shared-script execution is now recorded as required future work instead of remaining only in discussion

### Fixed

* Fixed the planning gap where station-grade needs such as shared instrument/channel locks and per-DUT log traceability were not explicitly captured as must-do items in repository documentation

### Notes

* This change updates the project plan and change history only; runtime multi-DUT support is not implemented yet

## [2026-04-12 08:56]

### Added

* Added standardized `MeasurementSet` and `MeasurementItem` result data so one script/device command can persist multiple parsed measurements together with `source`, `command`, `collectedAt`, `rawPayload`, and per-item `fullKey`
* Added per-step `SpecEvaluationResult` lists in the documented result contract so one step can report multiple independent rule outcomes against the same measurement batch

### Modified

* Updated recipe/spec documentation to describe step-owned `prefix` values, exact `targetKey` to `fullKey` matching, multi-measurement recipe shape, and the current `result.json` / `result.csv` / `session.log` output structure
* Updated getting-started and CLI overview guidance to include the runnable multi-measurement sample and the compatibility behavior for legacy single-value scripts

### Fixed

* Fixed the previous documentation gap where the user manual still described fullKey behavior broadly but did not explain the normalized `MeasurementSet` artifact model or per-step multi-rule output clearly enough
* Fixed the previous onboarding gap where users could not easily tell how repeated raw keys such as `voltage` and `current` stay collision-free when the same script shape is reused across multiple steps

### Notes

* Legacy Phase 1 / Phase 2 single-value recipes and specs remain compatible because the runtime auto-wraps old single-measurement steps into a one-item `MeasurementSet`

## [2026-04-05 10:58]

### Added

* Added minimal `sequence.outcomePolicy` support with deterministic `breakOnStepFailure` and `breakOnStepSuccess` behavior for container-level early termination
* Added focused automated coverage for sequence failure-break, success-break, backward-compatible default behavior, continue-on-failure interaction, and `FlowResultTree` stop metadata

### Modified

* Updated `FlowNodeExecutor`, `FlowEngine`, and `FlowResultTree` emission so sequence containers can stop early without changing the existing step execution contract
* Updated container structured-event metadata and recipe documentation to surface `outcomePolicy`, `stopReason`, and the child node that triggered a sequence break

### Fixed

* Fixed the previous container-policy gap where only `repeatUntil` supported deterministic policy-driven short-circuit behavior
* Fixed the previous observability gap where sequence-level early termination could not be traced explicitly in `FlowResultTree` or structured container events

### Notes

* Default sequence behavior remains unchanged when `outcomePolicy` is omitted
* This phase only adds minimal container-level termination policy for sequences and does not introduce generic break/continue statements or a broader workflow DSL

## [2026-04-05 10:44]

### Added

* Added minimal `repeatUntil.outcomePolicy` support with deterministic `breakOnStepFailure` and `breakOnStepSuccess` behavior
* Added explicit policy-driven repeat stop reasons such as `StepFailureBreak` and `StepSuccessBreak` to flow execution and `FlowResultTree`
* Added focused automated coverage for failure-break, success-break, backward-compatible default behavior, policy-vs-maxIterations precedence, and policy stop-reason output

### Modified

* Updated `repeatUntil` execution so policy-driven termination can short-circuit the loop without changing the existing default `until` / `maxIterations` semantics when no policy is configured
* Updated recipe loading, validation, and loop structured-event metadata to recognize the new repeat outcome-policy field
* Updated CLI user manual pages to document the new `repeatUntil.outcomePolicy` schema, supported values, stop reasons, and troubleshooting guidance

### Fixed

* Fixed the previous loop-policy gap where repeat execution could only stop by condition satisfaction, hard iteration failure, or max-iterations exhaustion
* Fixed the previous observability gap where policy-driven early loop termination could not be expressed explicitly in `FlowResultTree.StopReason`

### Notes

* Default behavior is backward compatible when `outcomePolicy` is omitted
* This phase only adds deterministic repeat termination policy and does not introduce generic break/continue statements or broader workflow language features

## [2026-04-05 10:36]

### Added

* Added explicit `FlowResultTree` output to `result.json` so execution artifacts now preserve sequence boundaries, condition branch choices, repeat iterations, node timing, and stop reasons
* Added `FlowNodeResult` and `FlowIterationResult` models for step, sequence, condition, and `repeatUntil` execution results without removing the existing flat `Steps` and `Scripts` summaries
* Added focused automated coverage for nested sequence tree shape, selected condition branch capture, repeat iteration results, max-iterations stop reasons, and persisted JSON round-trip behavior

### Modified

* Updated `FlowNodeExecutor`, `FlowEngine`, and `TestRunner` to build a structured execution result tree while keeping current step, branch, and repeat semantics unchanged
* Updated `script run` result emission so it keeps single-step execution semantics while still exporting a node-level flow result section for the executed path
* Updated CLI user manual pages to document the new `FlowResultTree` section in `result.json` and how it relates to existing flat summaries and structured events

### Fixed

* Fixed the previous artifact gap where richer flow execution semantics existed at runtime but were flattened away in `result.json`
* Fixed the previous troubleshooting gap where container nesting, selected branches, repeat stop reasons, and intermediate-versus-final node outcomes were only inferable from logs instead of explicit result data

### Notes

* The new result tree is additive and backward compatible; existing `Steps`, `Scripts`, `result.csv`, and `session.events.jsonl` remain available
* This phase does not add any new control-flow behavior; it only makes the existing flow execution results explicit

## [2026-04-05 08:58]

### Added

* Added minimal `repeatUntil` flow-tree support so recipes can poll or re-run child nodes until a narrow deterministic condition becomes true
* Added structured loop events for `LoopStarted`, `LoopIterationStarted`, `LoopIterationCompleted`, `LoopConditionEvaluated`, `LoopMaxIterationsReached`, and `LoopCompleted`
* Added focused automated coverage for repeat-until success, `dataExists` / `dataEquals` stop conditions, deterministic max-iteration failure, sequence nesting, and loop structured events

### Modified

* Updated flow-tree loading, validation, and execution to recognize `repeatUntil` nodes with child nodes plus `until`, `maxIterations`, and `failOnMaxIterations` settings
* Updated final flow-status calculation so non-terminal failed loop iterations remain visible in artifacts and logs without forcing the overall run to fail after a later successful iteration
* Updated CLI user manual pages to document the new repeat node schema, validation rules, loop events, and the unchanged single-step behavior of `script run`

### Fixed

* Fixed the previous production-core gap where polling or wait-until-ready style behavior required ad-hoc script design instead of an explicit deterministic loop foundation in the flow tree
* Fixed the previous orchestration gap where adding repeat behavior would have pushed loop logic back into `FlowEngine` instead of keeping flow-node orchestration separate from step and device execution

### Notes

* This phase remains single-DUT, deterministic, offline, and `FakeDevice`-based
* `repeatUntil` intentionally reuses the narrow condition model and does not introduce generic `while`, `goto`, parallelism, or a general-purpose expression engine

## [2026-04-05 08:42]

### Added

* Added minimal flow-tree recipe foundation with explicit `flow` sequence containers, `step` nodes, and deterministic `condition` nodes on top of the existing single-DUT execution core
* Added recursive `FlowNodeExecutor`, narrow condition evaluation, and structured container/branch events for `ContainerStarted`, `ContainerCompleted`, `BranchEvaluated`, and `BranchSelected`
* Added focused automated coverage for nested sequence execution order, continue-on-failure inside sequences, true/false branch selection, retry inside a branch sequence, and required container/branch structured events

### Modified

* Updated `RecipeLoader` and `RecipeValidator` to understand nested flow nodes, script-node references, and the supported condition types `previousStepStatus`, `dataExists`, and `dataEquals`
* Updated `FlowEngine` to orchestrate legacy flat script lists or the new optional `flow` tree without re-inlining step execution logic
* Updated CLI user manual pages to document the new `flow` recipe schema, branch-condition limits, and `script run` behavior when a recipe defines a flow tree

### Fixed

* Fixed the previous production-core gap where recipes could only express a flat sequential step list and had no explicit container or branch foundation for future flow growth
* Fixed the previous orchestration gap where nested execution would have required pushing container/branch logic back into `FlowEngine` instead of keeping step, node, and device boundaries explicit

### Notes

* This phase remains deterministic, single-DUT, offline, and `FakeDevice`-based
* The branch foundation is intentionally narrow and does not introduce loops, goto, or a general-purpose expression engine

## [2026-04-05 08:14]

### Added

* Added execution-core contracts for production flow behavior, including `IDeviceFactory`, `IDeviceSession`, `StepExecutionPolicy`, `ScriptExecutionRequest`, `DutExecutionRuntime`, and `DutExecutionState`
* Added `FlowStepExecutor` and deterministic step-policy handling for retry, timeout, continue-on-failure, and explicit step lifecycle logging
* Added focused automated coverage for transient retry recovery, continue-on-failure flow behavior, timeout handling, and injected device-factory execution

### Modified

* Updated `FlowEngine`, `RecipeScript`, `TestRunner`, and `DeviceExecutor` to use explicit device-session and step-execution boundaries instead of letting execution concerns remain flow-engine-only
* Updated session reporting and structured logs so retry counts, step timeout events, and retry events are carried into artifacts and logs
* Updated CLI user manual pages to document recipe step execution-policy fields `retryCount`, `timeoutMs`, and `continueOnFailure`

### Fixed

* Fixed the previous execution-core gap where flow behavior had no explicit retry/timeout policy contract and device creation was hard-wired to `FakeDevice` at call sites
* Fixed the previous boundary gap where immutable DUT metadata and mutable DUT execution state were mixed together instead of being separated into explicit runtime models

### Notes

* This slice stays CLI-first, deterministic, and `FakeDevice`-based while preparing the production-test core for future multi-DUT and real-device work
* The new recipe execution-policy fields are optional, backward compatible, and validated before execution

## [2026-04-05 07:52]

### Added

* Added Phase 3C.1 deterministic baseline/candidate regression checking with `AiRegressionCheckResult`, `AiRegressionFinding`, and `AiRegressionStatus`
* Added offline regression evaluation and reporting with `AiRegressionChecker`, `AiRegressionHtmlRenderer`, and `AiRegressionHtmlWriter`
* Added CLI command `ats ai regress --baseline-bundle <file> --candidate-bundle <file> --output-json <file> --output-html <file>` for bundle-based regression checks
* Added focused automated coverage for success-to-success, success-to-step-failure, success-to-variable-resolution-failure, summary-count regression, regression HTML rendering, and CLI regression outputs

### Modified

* Updated CLI usage/help output to include the new `ai regress` workflow and its required JSON/HTML outputs
* Updated the artifact-based AI workflow documentation to describe regression checking alongside analyze, render, and compare commands
* Updated offline HTML reporting to include a dedicated regression report with status, findings, metadata, and compared facts

### Fixed

* Fixed the previous Phase 3B.3 gap where operators could compare two bundles but still had no deterministic pass/fail style regression decision between baseline and candidate runs
* Fixed the previous baseline-review gap where worsening categories, increased failed-spec counts, new failed steps, and more severe matched rules had to be interpreted manually instead of being classified into explicit regression findings

### Notes

* `ats ai regress` is deterministic and fully offline; it does not call any provider, network service, or execution-engine runtime
* The command currently reports regressions through JSON/HTML artifacts and console output, but it does not change the CLI success exit code based on findings

## [2026-04-05 07:23]

### Added

* Added Phase 3B.3 multi-run bundle comparison support with `AiAnalysisBundleComparisonBuilder`, `AiAnalysisComparisonHtmlRenderer`, and `AiAnalysisComparisonHtmlWriter`
* Added CLI command `ats ai compare --left-bundle <file> --right-bundle <file> --output-html <file>` for offline two-run comparison reports
* Added focused automated coverage for same-bundle comparison, category changes, summary count changes, matched rule changes, comparison HTML rendering, and compare CLI execution

### Modified

* Updated CLI usage/help output to include the new `ai compare` command and its bundle-based input contract
* Updated the offline HTML reporting layer to show left-vs-right summary cards plus changed fields, summary counts, failed steps, matched rules, evidence, and recommended actions
* Updated user manual pages to document the new compare workflow, parameters, examples, expected output, limitations, and troubleshooting guidance

### Fixed

* Fixed the previous Phase 3B.2 gap where operators could inspect a single bundle offline but had no readable artifact-based way to compare two completed runs side by side
* Fixed the previous review gap where changes in rules, evidence, failed steps, and normalized counters required manual JSON diffing instead of an offline HTML comparison report

### Notes

* `ats ai compare` is fully offline and artifact-based; it does not call providers, start a web server, or interact with the execution engine
* The comparison input contract remains `analysis-bundle.json`, so raw `session.events.jsonl` streams are still excluded from the comparison report

## [2026-04-04 22:29]

### Added

* Added Phase 3B.2 interactive offline viewer behavior to the generated HTML report, including collapsible sections, inline search, severity/category filters, and summary count cards
* Added focused automated coverage for interactive section markup and search/filter rendering in the HTML viewer output

### Modified

* Updated `AiAnalysisHtmlRenderer` to generate a self-contained interactive HTML viewer using inline CSS and JavaScript only
* Updated `ats ai render` documentation to describe offline interactivity while preserving readable no-JavaScript output
* Updated overview documentation to position the viewer as an interactive offline HTML artifact rather than a static page only

### Fixed

* Fixed the previous Phase 3B.1 limitation where the viewer could render analysis data but could not be searched or narrowed without manually scanning the full page
* Fixed the previous usability gap where large evidence or summary-fact sections had no built-in offline way to collapse, search, or filter content

### Notes

* The viewer remains fully offline and does not introduce a web server, UI framework, or external assets
* All content remains readable when JavaScript is unavailable because sections stay expanded by default and the full report markup is still present

## [2026-04-04 22:14]

### Added

* Added Phase 3B.1 static bundle viewer support with `AiAnalysisHtmlRenderer` and `AiAnalysisHtmlWriter` for offline HTML report generation
* Added CLI command `ats ai render --bundle-json <file> --output-html <file>` to render `analysis-bundle.json` into a standalone HTML viewer artifact
* Added focused automated coverage for HTML generation, required viewer sections, CLI render execution, and backward compatibility of existing `ats ai analyze` flows

### Modified

* Updated CLI usage/help output to include the new `ai render` command and its bundle-first input contract
* Updated user documentation to describe bundle-first HTML rendering, static viewer output, and the new command syntax
* Updated overview documentation to list HTML viewer artifacts alongside the existing JSON, bundle, and console analysis outputs

### Fixed

* Fixed the previous Phase 3A handoff gap where analysis bundles could be exported but not viewed as a readable offline report without opening raw JSON
* Fixed the previous operator usability gap where explainable analysis data had no static report format for quick review or artifact sharing

### Notes

* `ats ai render` is fully static and offline; it does not start a web server or introduce a UI framework app
* The primary render input contract is `analysis-bundle.json`, not raw engine state or live analysis sessions

## [2026-04-04 22:04]

### Added

* Added Phase 3A.5 provider adapter placeholder contracts with `IAiBundleAnalysisProvider`, `AiProviderRequest`, and `AiProviderResponse`
* Added deterministic offline `FakeBundleAnalysisProvider` that consumes `AiAnalysisBundle` and produces provider-style summary output without any network calls
* Added optional CLI support for `ats ai analyze --provider fake` so bundle-based provider adapter behavior can be exercised without changing the default analysis path
* Added focused automated coverage for fake provider response generation, bundle/provider consistency, CLI provider execution, and backward compatibility when no provider is requested

### Modified

* Updated `ats ai analyze` to build an in-memory bundle when provider mode is requested, preserving the existing `result -> summary -> analysis -> bundle -> provider` layering
* Updated CLI console output and usage/help text to include provider placeholder output only when `--provider` is supplied
* Updated user documentation to describe the provider placeholder flow, syntax, examples, and current limitations

### Fixed

* Fixed the previous Phase 3A.4 gap where there was no narrow adapter layer to consume the exported bundle contract in a provider-ready shape
* Fixed the previous integration handoff gap where downstream provider experiments could not be exercised without modifying the deterministic analysis pipeline itself

### Notes

* `--provider fake` remains a deterministic placeholder only and does not call any external SDK or service
* Default `ats ai analyze` behavior is unchanged when `--provider` is not specified

## [2026-04-04 21:53]

### Added

* Added Phase 3A.4 provider-ready analysis bundle contract with `AiAnalysisBundle`, including schema version, artifact metadata, normalized summary, and explainable analysis output
* Added application-side `AiAnalysisBundleBuilder` and `AiAnalysisBundleWriter` for stable bundle creation and JSON writing
* Added optional CLI support for `ats ai analyze --output-bundle-json <file>` to export a standalone provider-ready analysis bundle artifact
* Added focused automated coverage for bundle metadata population, bundle JSON writing, bundle content round-trip, and CLI bundle export behavior

### Modified

* Updated `RunAnalysisService` to support bundle creation without exposing raw `session.events.jsonl` streams to analyzers or bundle consumers
* Updated CLI usage/help text and analysis flow to preserve existing `--output-json` behavior while optionally emitting a full bundle artifact
* Updated user documentation to describe the new bundle schema purpose, CLI syntax, examples, and output behavior

### Fixed

* Fixed the previous Phase 3A.3 limitation where normalized summary, explainable analysis, and artifact metadata were exported as separate pieces instead of one explicit provider-ready contract
* Fixed the previous handoff gap where downstream tooling had no stable single-file bundle to consume for future provider integration

### Notes

* The bundle remains explicit and normalized; it does not embed raw `session.events.jsonl` line streams
* Phase 3A.4 stays fully offline, deterministic, and CLI-first

## [2026-04-04 21:43]

### Added

* Added Phase 3A.3 explainable analysis evidence with `AiEvidenceItem`, rule-emitted evidence, and matched-rule tracking in `AiRunAnalysisResult`
* Added optional CLI export support for `ats ai analyze --output-json <file>` to write a standalone analysis artifact
* Added automated coverage for evidence generation, precedence-and-evidence consistency, and JSON analysis artifact writing

### Modified

* Updated `RuleBasedRunAnalyzer` to merge structured evidence from matched deterministic rules into the final analysis result
* Updated CLI `ats ai analyze` output to print an `Evidence` section alongside the existing classification, summary, observations, and recommended actions
* Updated user documentation to describe explainable analysis output and standalone JSON export usage

### Fixed

* Fixed the previous Phase 3A.2 limitation where the analyzer reported a primary cause without showing the normalized facts that caused the rule match
* Fixed the previous operator workflow gap where analysis results could not be saved as a dedicated portable JSON artifact for later review

### Notes

* Evidence remains normalized and explicit; raw event streams are still not passed directly into analyzers
* Explainable output stays fully deterministic, offline, and CLI-first

## [2026-04-04 21:37]

### Added

* Added Phase 3A.2 deterministic rule-based run analysis with `RuleBasedRunAnalyzer` and `IRunAnalysisRule`
* Added primary run-classification fields to `AiRunAnalysisResult`, including `PrimaryCategory`, `PrimaryCause`, `RecommendedActions`, and `Confidence`
* Added focused rule coverage for variable resolution failures, unhandled exceptions, step failures, spec failures, and successful runs
* Added automated tests for mixed-failure precedence so configuration/runtime faults outrank step/spec summaries

### Modified

* Updated `RunAnalysisService` to use the new rule-based analyzer by default instead of the placeholder summarizer
* Updated CLI `ats ai analyze` output to print primary category, primary cause, summary, observations, confidence, and recommended next actions
* Updated user documentation to describe the new deterministic classification behavior and output contract

### Fixed

* Fixed the previous Phase 3A limitation where analysis produced observations only, without a deterministic primary cause classification
* Fixed the previous usability gap where operators had to infer next actions manually from raw observations instead of getting explicit rule-based recommendations

### Notes

* The analyzer remains fully offline, deterministic, and summary-driven
* Failure precedence currently favors variable resolution failure over exception, exception over step failure, step failure over spec failure, and success last

## [2026-04-04 21:06]

### Added

* Added Phase 3A.1 event-aware run analysis fields to `RunArtifactSummary`, including normalized variable-resolution, exception, warning, and first-failure metadata
* Added optional CLI support for `ats ai analyze --result-json <file> --events-jsonl <file>`
* Added automated coverage for result-plus-events analysis, variable resolution failure analysis, exception analysis, and event-derived failed step normalization

### Modified

* Updated `ArtifactSummaryBuilder` to derive normalized event-aware analysis data from `session.events.jsonl` without exposing raw event streams to analyzers
* Updated `FakeRunAnalyzer` to emit deterministic observations and recommendations when structured events indicate variable resolution failures or unhandled exceptions
* Updated Phase 3A CLI documentation to describe optional structured-event input and event-aware placeholder behavior

### Fixed

* Fixed the previous Phase 3A limitation where `ats ai analyze` could only reason from `result.json` and ignored structured runtime evidence in `session.events.jsonl`
* Fixed the previous analyzer blind spot where variable resolution failures and exception-style runtime errors could not produce targeted deterministic recommendations

### Notes

* `--events-jsonl` remains optional so existing `ats ai analyze --result-json <file>` usage stays compatible
* Event-aware analysis remains deterministic, offline, and CLI-only

## [2026-04-04 20:33]

### Added

* Added Phase 3A narrow run-analysis contracts with `IAiRunAnalyzer`, `AiRunAnalysisRequest`, `AiRunAnalysisResult`, `AiObservation`, and `RunArtifactSummary`
* Added deterministic placeholder analysis components `ArtifactSummaryBuilder`, `FakeRunAnalyzer`, and `RunAnalysisService` that analyze normalized `result.json` summaries instead of runtime engine state
* Added minimal CLI command `ats ai analyze --result-json <file>` for console-only run analysis without UI or network-based AI integration
* Added automated coverage for artifact summary normalization, deterministic analyzer findings, and end-to-end result artifact analysis

### Modified

* Updated CLI usage/help output and user manual pages to include the new run-analysis command and Phase 3A positioning
* Updated project documentation to describe `ats ai analyze` as a deterministic placeholder layer built on existing run artifacts and structured outputs

### Fixed

* Fixed the previous Phase 3 direction ambiguity by constraining the new AI surface to run analysis instead of a general assistant abstraction
* Fixed the previous coupling risk by ensuring analyzers consume normalized artifact summaries rather than flow-engine internals

### Notes

* Phase 3A remains CLI-first and does not introduce UI, network calls, or external AI providers
* `ats ai analyze` currently supports `test simulate`, `test run`, and `script run` result artifacts only

## [2026-04-04 19:39]

### Added

* Added immutable `DutContext` support to the execution-time variable system with canonical `${dut.id}`, `${dut.index}`, `${dut.sn}`, `${dut.station}`, `${dut.slot}`, and `${dut.isSimulated}` placeholders
* Added recipe-validation warnings that mark supported `dut.*` placeholders as runtime-provided instead of treating them as missing during `ats recipe validate`
* Added automated coverage for canonical DUT variable resolution, no-fallback behavior, validation warnings, and enriched variable structured-log fields

### Modified

* Updated `VariableContext` to carry `StepVariables`, `DutContext`, and `GlobalVariables` while preserving explicit source tracking for structured log events
* Updated `VariableResolved` structured log payloads to include `requestedName`, `resolvedName`, `scope`, `value`, and `source`
* Updated CLI and architecture documentation to explain canonical `dut.*` behavior, supported runtime inputs, and validate-mode warnings

### Fixed

* Fixed the previous gap where DUT identity data could only be referenced indirectly through global variables instead of explicit `dut.*` placeholders
* Fixed the previous observability gap where variable structured logs could not distinguish the originally requested name from the canonical resolved DUT field

### Notes

* Single-DUT CLI flows now build `DutContext` from existing run input: `--sn` feeds `dut.sn`, `--station` feeds `dut.station`, `--vars DutId=...`, `DutIndex=...`, and `Slot=...` feed the remaining fields, and `dut.isSimulated` reflects the command mode
* `dut.*` placeholders resolve only from `DutContext`; Step and Global variables do not override them

## [2026-04-04 11:20]

### Added

* Added Phase 2 execution-time variable system core with `VariableScope`, `ResolvedVariable`, `VariableContext`, and `VariableResolver`
* Added recipe-level and step-level `variables` support plus `${varName}` resolution for step command text, simulated response text, and measurement string fields such as `sourcePath`, `unit`, and `description`
* Added structured log coverage for variable resolution through `VariableResolved` and `VariableResolutionFailed` events in `session.events.jsonl`
* Added `samples/recipes/variable-system.recipe.json` as a runnable CLI-first sample for execution-time variable resolution
* Added automated coverage for Step>DUT>Global precedence, missing-variable failures, malformed variable template validation, resolved command/payload behavior, and variable structured-log events
* Added architecture documentation for the execution-time variable system contract in `docs/architecture/variable-system.md`

### Modified

* Updated flow execution so each step resolves a runtime step snapshot before command dispatch, keeping actual executed command and parsed measurement metadata aligned in results and logs
* Updated recipe validation to reject malformed `${varName}` syntax in supported step and measurement template fields before execution starts
* Updated CLI user manual pages to document `${varName}` behavior, supported fields, scope order, and runtime failure troubleshooting

### Fixed

* Fixed the previous gap where recipe strings could not consume execution-time values such as SN without hardcoding them into scripts or raw payloads
* Fixed the previous observability gap where future UI/tooling could not explain which scope supplied a resolved runtime value

### Notes

* Execution-time `${varName}` resolution is separate from artifact naming templates like `%SN%` and `{SN}`
* Phase 2 keeps single-DUT compatibility by allowing an empty DUT scope while preserving the fixed lookup order Step > DUT > Global

## [2026-04-04 09:30]

### Added

* Added versioned structured log output in `session.events.jsonl` using schema `ats.structured-log.v1` with session-global increasing `sequence` for timeline replay
* Added `SessionArtifactManifest` and expanded `SessionInfo` so session metadata, input values, and artifact paths have a canonical source in result payloads
* Added structured log event coverage for session start, input capture, recipe start/completion, measurement collection, DataCollection writes, spec evaluation, step completion, artifact writes, and session completion
* Added automated coverage to verify structured log schema version, global sequence ordering, and canonical `SessionInfo` behavior

### Modified

* Updated CLI output and session reports to expose the machine-readable structured log artifact path alongside `result.json`, `result.csv`, and `session.log`
* Updated session header and session result report sections to include the structured log path
* Documented `DataCollection` as `last-write-wins` while preserving every write event in the structured log artifact

### Fixed

* Fixed the previous result contract ambiguity by making `SessionInfo` the canonical session metadata source while preserving top-level compatibility mirrors
* Fixed the previous UI/tooling gap where only human-readable session logs existed and no stable structured event timeline was available

### Notes

* `session.log` remains the human-readable operator log, while `session.events.jsonl` is the machine-readable structured log intended for future UI, tools, and AI workflows

## [2026-04-04 08:45]

### Added

* Added formal run input support for serial number, station, mode, and additional CLI input values through `RunInputModel` and `SessionInfo`
* Added CLI `--sn` and `--prompt-sn` support so production-style runs can accept a serial number before execution without requiring UI
* Added `{Token}` artifact naming support alongside existing `%Token%`, including `{SN}`, `{SessionId}`, `{Recipe}`, `{Station}`, `{Mode}`, and direct date format tokens such as `{yyyyMMdd_HHmmss}`
* Added unique per-session log naming by default so each run produces its own log file even when the same output folder is reused
* Added richer session result blocks in `session.log` with recipe, station, mode, input values, final result, start/end time, duration, and artifact paths

### Modified

* Extended test, script, device, recipe validate, and spec validate CLI parsing to support interactive serial-number entry and structured run input propagation
* Updated session artifact resolution to merge CLI input values into naming templates and to normalize `{Recipe}` to a cleaner recipe identifier
* Updated `result.json` payloads to include explicit run input and session metadata in addition to the existing flat fields
* Updated user manual pages to document `--sn`, `--prompt-sn`, session-style log output, and brace-based naming templates

### Fixed

* Fixed the previous run flow where serial number input only existed as an ad-hoc template variable and was not recorded as formal session input
* Fixed the previous logging behavior where session metadata such as station, mode, final artifact paths, and final session summary were not consistently captured

### Notes

* Existing `%VariableName%`, `--product-sn`, and `--prompt-vars ProductSN` behavior remains compatible, but `--sn` and `--prompt-sn` are now the primary CLI entry points

## [2026-04-04 00:15]

### Added

* Added CLI artifact templating options for output folder, JSON, CSV, and session log paths with `%VariableName%` expansion and automatic directory creation
* Added support for user-supplied template variables through CLI `--vars`, including PowerShell-safe comma-separated usage
* Added session log header data for application version, release time, machine information, config file details, login user, CPU, memory, IP, and MAC address
* Added a `Test Summary` table to `session.log` showing item name, result, test time, elapsed runtime, retry count, and measured data summary
* Added automated coverage for templated artifact paths, immediate session log persistence, and the new session log header/summary format

### Modified

* Changed session logging to write incrementally to the final log file path instead of waiting until the run ends, improving crash-time diagnostics
* Extended CLI output to print the actual resolved artifact paths after template expansion
* Extended test, script, device, recipe validate, and spec validate command help and user manual pages to document artifact templates and session log format
* Added per-step timing fields to step results so elapsed and per-item durations can be reported in artifacts and log summaries

### Fixed

* Fixed the previous artifact behavior where custom folder structures and file naming patterns could not be configured from the CLI
* Fixed the previous debugging gap where a process crash before final artifact writing could leave `session.log` empty or incomplete

### Notes

* Template variables resolve to `NA` when not provided, and PowerShell users should quote `--vars` or use comma-separated assignments such as `"ProductSN=SN001,LoginUser=MTE"`

## [2026-04-03 23:48]

### Added

* Added readable `session.log` formatting with local timestamps, elapsed runtime to milliseconds, log level, and `item=` labels so each line clearly shows the related step or command
* Added automated coverage to verify session log timestamp, elapsed runtime, and item-name formatting

### Modified

* Updated step-level measurement and spec evaluation logging to include the active test item name
* Updated CLI user manual pages to describe the new `session.log` timestamp and item labeling format

### Fixed

* Fixed the previous `session.log` format that used UTC ISO timestamps and did not clearly show which step or command produced each line

### Notes

* The new log format keeps artifact content text-based and simple while making run timing and step ownership much easier to read during troubleshooting

## [2026-04-03 23:30]

### Added

* Added a compact CLI rule summary format that separates measurement lines from pass/fail rule lines for easier operator scanning during runs

### Modified

* Simplified console rule output to `[PASS]/[FAIL] rule | targetKey | spec | code`, reducing repeated `actual` and label noise while keeping detailed values in artifacts
* Updated CLI manual pages to describe the streamlined console output format

### Fixed

* Fixed the previous console output clutter where measurement values and rule values repeated the same `actual` data on adjacent lines

### Notes

* `result.json`, `result.csv`, and `session.log` still retain the more detailed spec information when full troubleshooting detail is needed

## [2026-04-03 23:21]

### Added

* Added configured spec fields to `SpecEvaluationResult` so JSON and CSV outputs now include expected values, range limits, and regex patterns alongside actual values

### Modified

* Updated CLI rule output to show `targetKey`, rule type, configured spec values, actual value, and error code in one line
* Updated session logging to include configured spec summaries for each evaluation result

### Fixed

* Fixed the previous CLI/result output gap where users could see pass/fail status but not the configured spec threshold or expected value that caused it

### Notes

* This change keeps the existing fullKey-first flow and only makes the rule settings explicit in console, `result.json`, `result.csv`, and `session.log`

## [2026-04-03 23:16]

### Added

* Added `samples/recipes/all-spec-types-fail.recipe.json` as a runnable sample that intentionally fails multiple spec rules while preserving the same fullKey and operator coverage as the passing all-spec-types sample
* Added automated coverage to verify the fail sample returns overall `Failed`, preserves per-rule error codes, and keeps the bypass rule in `Passed`

### Modified

* Updated CLI documentation to show how to run the new fail sample with `test run` and `test simulate`

### Fixed

* Clarified the recommended sample for inspecting failed `SpecResults` output and `errorCode` behavior

### Notes

* The fail sample reuses `samples/specs/all-spec-types.spec.json` so the pass and fail recipes can be compared against the exact same rules

## [2026-04-03 23:13]

### Added

* Added `samples/recipes/all-spec-types.recipe.json` and `samples/specs/all-spec-types.spec.json` to demonstrate every supported spec rule type in one CLI-first sample run
* Added automated coverage for the all-spec-types sample run plus recipe/spec validation

### Modified

* Updated CLI user manual examples to show how to run the new all-spec-types sample with `test run`, `test simulate`, and `script run`

### Fixed

* Clarified the supported sample path for validating fullKey-prefixed measurements together with all spec operators

### Notes

* The new sample mixes step prefix override and recipe prefix fallback so users can inspect both `battery.*` and `dut.*` fullKey behavior in one run

## [2026-04-03 22:17]

### Added

* Added automated coverage for prefixed `fullKey` coexistence, empty-prefix legacy compatibility, duplicate `fullKey` validation, missing `targetKey` validation, and concurrent `DataCollection` access

### Modified

* Formalized `fullKey`-first measurement handling so internal lookup, spec matching, and evaluation use the exact `fullKey` while preserving `key` as the original field name
* Strengthened recipe validation to require exact `SpecRule.targetKey` to declared measurement `fullKey` mappings and to reject duplicate `fullKey` values within a measurement set and across recipe steps
* Updated CLI user manual pages to document prefix/fullKey behavior, exact spec target matching, and artifact output expectations

### Fixed

* Fixed `DataCollection` writes to be safe for concurrent access during measurement storage
* Fixed runtime measurement building to reject duplicate `fullKey` values before spec evaluation starts

### Notes

* This formalizes the existing prefix namespace mechanism without breaking legacy recipes or specs because empty prefix still produces `fullKey == key`

## [INIT]

### Added

* Repository initialized
* README.md created
* CODEX_PLAN.md created
* AGENTS.md created
* CHANGELOG.md created

### Notes

* Initial empty project setup

## [2026-03-31 23:16]

### Added

* Added `ATS.Core`, `ATS.Application`, `ATS.Cli`, and `ATS.Tests` for the Phase 1 CLI-first MVP
* Added `ScriptBase`, `TestContext`, `DataCollection`, `TestResult`, `IDevice`, `FakeDevice`, `FlowEngine`, `RecipeLoader`, and `SpecEngine`
* Added sample recipe `samples/recipes/demo.recipe.json`
* Added CLI artifact generation for `result.json`, `result.csv`, and `session.log`

### Modified

* Updated CLI documentation to describe the actual Phase 1 command surface and output behavior
* Updated getting started documentation with build, run, exit code, limitation, and troubleshooting details

### Fixed

* Fixed the repository state from documentation-only scaffolding to a runnable `ats test simulate` implementation

### Notes

* Phase 1 was implemented as a minimal CLI-first simulation flow with `FakeDevice` and no UI

## [2026-03-31 23:50]

### Added

* Added Phase 2 CLI commands: `ats test run`, `ats script run`, `ats device exec`, `ats recipe validate`, and `ats spec validate`
* Added `DeviceCommandRequest` / `DeviceCommandResponse`, session artifact writing, recipe/spec validation services, and shared test runner infrastructure
* Added `samples/recipes/phase2.recipe.json` and `samples/specs/phase2.spec.json` for Phase 2 execution and validation examples

### Modified

* Strengthened `FlowEngine`, `TestRunner`, and `SpecEngine` to preserve artifacts on failures and support `Equal`, `Range`, `GreaterThan`, `LessThan`, `Regex`, `Contain`, `NotEqual`, and `Bypass`
* Expanded CLI parsing, help text, and exit code handling while preserving Phase 1 `ats test simulate`
* Updated user manual pages to document Phase 2 commands, parameters, examples, outputs, exit codes, limitations, and troubleshooting notes

### Fixed

* Fixed failed or invalid runs so `result.json`, `result.csv`, and `session.log` remain available for debugging

### Notes

* Phase 2 remains CLI-first, Core-first, FakeDevice-only, and does not add UI
