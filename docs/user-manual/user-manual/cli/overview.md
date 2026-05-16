# CLI Overview

## Command Structure

```bash
ats <category> <action> [options]
```

---

## Commands

* `test simulate`
* `test run`
* `script run`
* `device exec`
* `recipe validate`
* `spec validate`
* `ai analyze`
* `ai render`
* `ai compare`
* `ai regress`

---

## Example

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

```bash
ats test run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

---

## Supported Options

* `--recipe <file>`: recipe JSON path for `test`, `script`, and `recipe` commands
* `--spec <file>`: optional external spec JSON path for `test`, `script`, and `recipe` commands
* `--script <name>`: required when using `ats script run`
* `--command <text>`: required when using `ats device exec`
* `--result-json <file>`: required when using `ats ai analyze`
* `--bundle-json <file>`: required when using `ats ai render`
* `--left-bundle <file>`: required left-side bundle input for `ats ai compare`
* `--right-bundle <file>`: required right-side bundle input for `ats ai compare`
* `--baseline-bundle <file>`: required baseline bundle input for `ats ai regress`
* `--candidate-bundle <file>`: required candidate bundle input for `ats ai regress`
* `--events-jsonl <file>`: optional structured log artifact for richer `ats ai analyze` findings
* `--output-json <file>`: optional analysis artifact path for `ats ai analyze`, required regression JSON artifact path for `ats ai regress`
* `--output-bundle-json <file>`: optional provider-ready analysis bundle path for `ats ai analyze`
* `--provider fake`: optional deterministic provider placeholder for `ats ai analyze`
* `--output-html <file>`: required HTML artifact path for `ats ai render`, `ats ai compare`, and `ats ai regress`
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Variable Resolution

Execution-time recipe variables use `${varName}`.

Resolution order is fixed:

1. Step variables
2. DUT variables
3. Global variables

Canonical DUT placeholders are:

* `${dut.id}`
* `${dut.index}`
* `${dut.sn}`
* `${dut.station}`
* `${dut.slot}`
* `${dut.isSimulated}`

Single-DUT CLI flows build `DutContext` from the current run input:

* `--sn` feeds `dut.sn`
* `--station` feeds `dut.station`
* `--vars DutId=...`, `DutIndex=...`, and `Slot=...` feed the remaining DUT fields
* `dut.id` falls back to the active serial number when `DutId` is not supplied
* `dut.isSimulated` reflects the active command mode

If a placeholder starts with `dut.`, it resolves only from `DutContext`. Step and Global variables do not override canonical DUT names.

Use execution-time variables in recipe fields such as:

* step `command`
* step `simulatedResponse`
* measurement `sourcePath`
* measurement `unit`
* measurement `description`

Do not confuse execution-time `${varName}` with artifact naming templates:

* `%SN%`
* `{SN}`

Artifact naming templates only change output paths.

Execution-time variables change what the step actually runs and parses.

---

## Output Types

* console output
* JSON
* CSV
* `session.log` for human-readable operator logs
* `session.events.jsonl` for machine-readable structured event logs
* `FlowResultTree` inside `result.json` for explicit node-level execution structure
* deterministic console analysis from `ats ai analyze`
* optional standalone JSON analysis artifact from `ats ai analyze --output-json`
* optional normalized bundle artifact from `ats ai analyze --output-bundle-json`
* optional provider placeholder output from `ats ai analyze --provider fake`
* optional interactive offline HTML viewer artifact from `ats ai render --output-html`
* optional offline HTML comparison report from `ats ai compare --output-html`
* optional regression JSON + offline HTML report from `ats ai regress`

---

## Measurement Model

Execution commands normalize device/script output into a `MeasurementSet`:

* `source`: step or script source name
* `command`: executed command text
* `collectedAt`: measurement collection time
* `rawPayload`: original device payload before parsing
* `items[]`: parsed `MeasurementItem` entries

Each `MeasurementItem` contains:

* `key`: original field name such as `voltage`
* `prefix`: recipe or step namespace such as `battery`
* `fullKey`: exact lookup key used by spec rules, such as `battery.voltage`
* `value`, `valueType`, `unit`, `description`, `rawText`

Prefix ownership stays in the recipe step, not in the script class or device adapter. If `prefix` is empty, `fullKey` stays equal to `key`.

Legacy single-value recipes remain compatible because the runtime auto-wraps the old `measurementKey` shape into a one-item `MeasurementSet`.

---

## Spec Evaluation Model

One step can evaluate multiple rules against one `MeasurementSet`.

Result artifacts keep this in two places:

* `StepResult.measurements`: normalized measurement list for the executed step
* `StepResult.specResults`: independent `SpecEvaluationResult` records for each rule

Each `SpecEvaluationResult` shows the rule name, exact `targetKey`, actual value, pass/fail result, `errorCode`, and reason text.

This means one command can return multiple fields and still produce a separate pass/fail trail for each required rule.

---

## Notes

* The system remains CLI-first; the new `ATS.Ui` desktop app is a thin shell that reads artifacts and triggers CLI commands instead of re-implementing core logic
* All execution commands use `FakeDevice`
* Production execution now uses explicit step policies and device-session boundaries for retry, timeout, continue-on-failure, container-level sequence breaks, and repeat-until loop behavior
* `test simulate` remains available for Phase 1 compatibility
* Phase 3A adds a narrow run-analysis layer that works from saved `result.json` artifacts
* Phase 3A.1 lets `ats ai analyze` optionally consume `session.events.jsonl` for richer deterministic observations
* Phase 3A.3 adds structured analysis evidence and optional standalone JSON export for explainable run analysis
* Phase 3A.4 adds a provider-ready bundle contract that packages normalized summary, explainable analysis, and artifact metadata into one JSON file
* Phase 3A.5 adds a provider adapter placeholder that consumes `AiAnalysisBundle` through `--provider fake` without introducing any external AI integration
* Phase 3B.1 adds a bundle-first offline viewer that renders `analysis-bundle.json` into a readable HTML report
* Phase 3B.2 adds inline offline interactivity to the HTML viewer, including collapsible sections plus search and filter controls
* Phase 3B.3 adds a two-run bundle comparison workflow that renders left-vs-right differences into an offline HTML report
* Phase 3C.1 adds a deterministic baseline/candidate regression check workflow that writes machine-readable JSON plus an offline HTML report
* `SessionInfo` is the canonical session metadata source in result payloads
* `DataCollection` currently uses `last-write-wins` for repeated `fullKey` writes; every write is still preserved as a structured log event in `session.events.jsonl`
* Missing execution-time variables fail the active run; they do not resolve to `NA`
* `ats recipe validate` keeps supported `dut.*` placeholders unresolved and reports them as runtime-provided warnings instead of validation errors
* `ats ai analyze` is deterministic rule-based behavior only; it does not call external AI services
* `ats ai render` is offline viewer generation only; it does not create a live UI or start a web server
* `ats ai compare` is offline comparison generation only; it does not rerun analysis or inspect the execution engine
* `ats ai regress` is offline regression classification only; it does not rerun analysis or currently fail the CLI exit code when regressions are found

---

## Production Execution Foundation

Recipe execution now has a small explicit policy contract:

* `retryCount`: retry a step after an execution error or timeout
* `timeoutMs`: cancel a step attempt after a deterministic timeout budget
* `continueOnFailure`: keep the flow moving after the final execution error

Supporting execution boundaries are now clearer:

* `DutContext` remains immutable metadata for variable resolution
* `DutExecutionRuntime` carries mutable DUT-scoped execution state such as active step and last error
* device creation flows through `IDeviceFactory`
* active command dispatch flows through `IDeviceSession`

The next execution layer now also supports a minimal flow tree:

* `flow.nodes[]` can execute `step`, `sequence`, `condition`, and `repeatUntil` nodes
* `condition` and `repeatUntil.until` currently support only `previousStepStatus`, `dataExists`, and `dataEquals`
* `sequence.outcomePolicy` can optionally short-circuit the current container on a passed or failed child using `breakOnStepSuccess` or `breakOnStepFailure`
* `repeatUntil` re-runs its child nodes until the stop condition is satisfied, or until `maxIterations` is reached
* `repeatUntil.outcomePolicy` can optionally short-circuit the loop on a passed or failed iteration using `breakOnStepSuccess` or `breakOnStepFailure`
* non-terminal failed loop iterations stay visible in artifacts and structured logs but do not force the final run status after a later successful iteration
* `result.json` now also emits a `FlowResultTree` section that preserves container boundaries, selected branches, repeat iterations, node timing, intermediate-versus-final status contribution, and policy-driven stop metadata such as `StopReason` and `TriggeredByNodeName`
* legacy recipes without `flow` still run `scripts` in declaration order

This keeps the current system single-DUT and `FakeDevice`-based while making future multi-DUT and real-device work easier to add without a large refactor.

---

## Exit Codes

* `0`: success
* `1`: test or script completed with failed spec result
* `2`: invalid CLI arguments
* `3`: recipe or spec validation failed
* `4`: runtime or device execution error
