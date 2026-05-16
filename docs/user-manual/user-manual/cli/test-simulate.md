# Command: test simulate

## Description

Run test flow in simulation mode without real hardware. One simulated step can still emit multiple measurements and evaluate multiple rules. Measurements use the same `fullKey` rules as live runs: step prefix overrides recipe prefix, and empty prefix keeps `fullKey` equal to `key`.

---

## Syntax

```bash
ats test simulate --recipe <file> [--spec <file>] [--output <directory>]
```

```bash
ats test simulate --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v,...>]
```

---

## Parameters

* `--recipe <file>`: path to recipe JSON file
* `--spec <file>`: optional external spec JSON file; if omitted, inline recipe specs are used
* `--output <directory>`: optional output directory, defaults to current working directory
* `--output-template <path>`: optional relative or absolute output folder template; supports `%Var%` and `{Var}` tokens such as `{SN}`, `{SessionId}`, `{Recipe}`, `{Station}`, `{Mode}`, and direct date format tokens such as `{yyyyMMdd_HHmmss}`
* `--json-template <path>`: optional result JSON path template, relative to the resolved output directory when not absolute
* `--csv-template <path>`: optional result CSV path template, relative to the resolved output directory when not absolute
* `--log-template <path>`: optional session log path template, relative to the resolved output directory when not absolute
* `--sn <value>`: optional product serial number recorded into the session and available to naming templates
* `--prompt-sn`: prompt for the serial number before the simulation starts
* `--station <name>`: optional station name recorded into the session and available to naming templates
* `--mode <name>`: optional mode label recorded into the session; defaults to `SIMULATE`
* `--vars <k=v,...>`: optional template variables for path expansion; PowerShell users should wrap the value in quotes or use commas instead of bare semicolons

---

## Multi-Measurement Compatibility

Simulation uses the same step schema as `test run`:

* step `prefix` defines the namespace for parsed measurement keys
* `measurements[]` declares multiple parsed fields from one raw payload
* spec `rules[]` target the exact emitted `fullKey`
* legacy single-value `measurementKey` steps still run and are auto-wrapped into a one-item `MeasurementSet`

---

## Recipe Variables

Execution-time recipe variables use `${varName}`.

Resolution order is fixed:

1. Step
2. DUT
3. Global

`test simulate` resolves `${varName}` in step commands, simulated responses, and measurement string fields such as `sourcePath`, `unit`, and `description`.

Canonical DUT variables are:

* `${dut.id}`
* `${dut.index}`
* `${dut.sn}`
* `${dut.station}`
* `${dut.slot}`
* `${dut.isSimulated}`

`dut.*` placeholders resolve only from `DutContext`; Step and Global variables do not override them.

Use `--sn`, `--station`, and `--vars "DutId=...,DutIndex=...,Slot=..."` to feed DUT values during simulation. `dut.id` falls back to the active serial number when `DutId` is omitted. `dut.isSimulated` is `true` for `test simulate`.

---

## Step Execution Policy

Simulation uses the same deterministic step policy fields as live `test run`:

* `retryCount`: retry a step after an execution error or timeout
* `timeoutMs`: cancel the active attempt when simulated execution exceeds the configured timeout
* `continueOnFailure`: continue to the next step after the final execution error

Example recipe step:

```json
{
  "name": "ReadVoltage",
  "command": "READ_VOLTAGE",
  "measurementKey": "battery.voltage",
  "unit": "V",
  "retryCount": 2,
  "timeoutMs": 250,
  "continueOnFailure": true,
  "simulatedResponse": "12.3"
}
```

---

## Flow Tree Foundation

`test simulate` can now execute an optional recipe `flow` tree while still using `scripts` as the reusable step catalog.

Supported node types:

* `step`
* `sequence`
* `condition`
* `repeatUntil`

Supported condition types:

* `previousStepStatus`
* `dataExists`
* `dataEquals`

`repeatUntil` runs its child nodes once per iteration, then evaluates `until`.

`sequence` can also declare a minimal `outcomePolicy` to stop the current container early after a passed or failed child result.

Supported repeat fields:

* `nodes`
* `until`
* `outcomePolicy`
* `maxIterations`
* `failOnMaxIterations`

Supported sequence fields:

* `nodes`
* `outcomePolicy`

Example repeat fragment:

```json
{
  "type": "repeatUntil",
  "name": "WaitForReady",
  "outcomePolicy": "breakOnStepSuccess",
  "maxIterations": 3,
  "failOnMaxIterations": true,
  "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
  "nodes": [
    { "type": "step", "step": "PollReady" }
  ]
}
```

Example sequence fragment:

```json
{
  "type": "sequence",
  "name": "GateSequence",
  "outcomePolicy": "breakOnStepFailure",
  "nodes": [
    { "type": "step", "step": "ReadGate" },
    { "type": "step", "step": "ReadAfterGate" }
  ]
}
```

If `flow` is omitted, simulation keeps the legacy flat `scripts` order.

---

## Example

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json --output artifacts/demo-run
```

```bash
ats test simulate --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

```bash
ats test simulate --recipe samples/recipes/multi-measurement.recipe.json --spec samples/specs/multi-measurement.spec.json
```

```bash
ats test simulate --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json
```

```bash
ats test simulate --recipe samples/recipes/all-spec-types-fail.recipe.json --spec samples/specs/all-spec-types.spec.json --output artifacts/all-spec-types-fail-sim
```

```bash
ats test simulate --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --output .codex-out --output-template runs\%ProductSN%\%CurTime% --log-template logs\%ProductSN%_%CurTime%.log --vars "ProductSN=SN001,LoginUser=MTE"
```

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json --prompt-sn --output .codex-out\prompt-run --log-template "logs\{SN}_{yyyyMMdd_HHmmss}.log"
```

```bash
ats test simulate --recipe samples/recipes/variable-system.recipe.json --sn SN-SIM-001 --output artifacts/variable-sim
```

```bash
ats test simulate --recipe samples/recipes/variable-system.recipe.json --sn SN-SIM-001 --station ST-SIM --vars "DutId=DUT-SIM-01,DutIndex=1,Slot=SIM01"
```

---

## Expected Output

* one `result.json` file for the session
* one `result.csv` file for the session
* one dedicated session log file for the session; the default log file name is `session_<SessionId>.log`
* one machine-readable structured event log file for the session; the default file name is `session_<SessionId>.events.jsonl`
* Measurement output includes `key`, `prefix`, and `fullKey`
* each step in `result.json` includes `measurementSet` with the original `rawPayload` plus parsed `items`
* each step also includes `measurements` and `specResults` lists so a single simulated command can show multiple field-level rule results
* Spec evaluation output uses exact `targetKey` fullKey values
* `result.json` includes `FlowResultTree` so simulated sequence/branch/repeat execution paths remain explicit alongside the existing flat summaries
* `FlowResultTree` sequence nodes can now record `OutcomePolicy`, `StopReason`, and `TriggeredByNodeName` when a container stops early
* `FlowResultTree` repeat nodes record stop reasons such as `ConditionSatisfied`, `MaxIterationsReached`, `IterationFailure`, `StepFailureBreak`, and `StepSuccessBreak`
* Console output keeps measurements on separate lines and prints a compact rule summary as `[PASS]/[FAIL] rule | targetKey | spec | code`
* `session.log` uses local time in `yyyy-MM-dd HH:mm:ss.fff` format and shows elapsed runtime plus `item=<step or flow name>` on every line
* `session.log` begins with a session header that includes Session ID, Start Time, Recipe file, Station, Mode, SN, output folder, and environment data
* `session.log` ends with a session result block and a `Test Summary` table containing item name, result, test time, elapsed runtime, retry count, and data summary
* `session.events.jsonl` contains versioned structured log entries with `schemaVersion`, session-global `sequence`, timestamps, entry type, status, and event data for UI/tool/AI replay
* `session.events.jsonl` records `StepRetried` and `StepTimedOut` events when simulation step policies trigger retry or timeout handling
* `session.events.jsonl` also records `ContainerStarted`, `ContainerCompleted`, `BranchEvaluated`, and `BranchSelected` when explicit flow-tree nodes are used
* `ContainerStarted` and `ContainerCompleted` include sequence `outcomePolicy`, `stopReason`, and `triggeredByNodeName` when a sequence container terminates early
* `session.events.jsonl` records `LoopStarted`, `LoopIterationStarted`, `LoopIterationCompleted`, `LoopConditionEvaluated`, `LoopMaxIterationsReached`, and `LoopCompleted` when `repeatUntil` nodes are used
* Variable-based recipes also emit `VariableResolved` and `VariableResolutionFailed` events in `session.events.jsonl`
* `VariableResolved` entries include `requestedName`, `resolvedName`, `scope`, `value`, and `source` for canonical DUT traceability
* Non-terminal failed loop iterations stay visible in artifacts and logs, but a later successful iteration can still produce an overall passing simulation result

---

## Exit Codes

* 0: all script results passed spec evaluation
* 1: one or more script results failed spec evaluation
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* Only JSON recipe and spec files are supported
* Only `FakeDevice` is available; no real hardware integration is included
* Spec rules must target the exact measurement `fullKey`; there is no fuzzy fallback from `key` to prefixed values
* When the same raw keys are reused across steps, use step or recipe prefixes to keep each emitted `fullKey` unique
* Unknown template variables resolve to `NA`
* Missing execution-time `${varName}` variables fail the simulation; they do not silently become empty strings or `NA`
* `retryCount` only applies to execution errors and timeouts; it does not retry spec-evaluation failures
* Flow conditions are intentionally narrow in this phase; `repeatUntil` does not add generic `while` syntax, parallelism, or a general-purpose expression engine
* `sequence.outcomePolicy` and `repeatUntil.outcomePolicy` only support `breakOnStepFailure` and `breakOnStepSuccess`
* `ats recipe validate` leaves supported `dut.*` placeholders unresolved and reports them as runtime-provided warnings
* If a template path collides with an existing file, the system automatically appends SessionId data so the new run does not overwrite the previous artifacts
* `DataCollection` uses `last-write-wins` if the same `fullKey` is written again; use `session.events.jsonl` to inspect every write event

---

## Troubleshooting

* If the command reports `Recipe file was not found`, verify the `--recipe` path
* If the command reports validation errors, verify the recipe/spec mapping and supported operators
* If validation reports missing or duplicate `fullKey` values, inspect the recipe prefixes and the declared measurement keys
* If a templated artifact path does not look right in PowerShell, wrap brace templates in quotes and wrap `--vars` in quotes or use comma-separated assignments such as `"LoginUser=MTE"`
* If a simulation fails because a `dut.*` variable is missing, inspect `session.events.jsonl` for `VariableResolutionFailed` and verify the value was supplied through `--sn`, `--station`, or `--vars DutId=...`, `DutIndex=...`, `Slot=...`
* If a simulation fails because a non-DUT variable is missing, inspect `session.events.jsonl` for `VariableResolutionFailed` and verify the value exists in Step or Global scope
* If a simulated step times out, inspect `session.events.jsonl` for `StepTimedOut` and verify the configured `timeoutMs` matches the expected payload or fake-device delay
* If a simulated step retries, inspect the `Test Summary` retry count and the `StepRetried` event data to confirm whether the recipe policy is intentionally allowing recovery
* If a simulated branch selects the wrong path, inspect `BranchEvaluated` and `BranchSelected` in `session.events.jsonl` to confirm the actual runtime value and selected branch
* If a simulated repeat loop keeps running, inspect `LoopConditionEvaluated` in `session.events.jsonl` to confirm the loop condition is reading the expected key or previous-step status
* If a simulated repeat loop ends with `LoopMaxIterationsReached`, increase `maxIterations` or change `failOnMaxIterations` only if the recipe is intentionally tolerant of incomplete readiness
* If a simulated repeat loop stops before the `until` condition matches, inspect `FlowResultTree.StopReason` and confirm whether `outcomePolicy` intentionally ended on a passed or failed iteration
* If a simulated sequence stops earlier than expected, inspect `FlowResultTree.StopReason`, `FlowResultTree.OutcomePolicy`, and `FlowResultTree.TriggeredByNodeName` to identify the child node that caused the break
* If the flat step list is not enough to explain nested simulated behavior, inspect `FlowResultTree` in `result.json` to see container nesting and repeat iterations directly
* If `--prompt-sn` appears to wait forever, verify your terminal can provide standard input and that your barcode scanner sends a trailing Enter key
* If output files are missing, verify the `--output` directory is writable
* If you need machine-readable step or measurement history, inspect `session.events.jsonl` instead of parsing `session.log`
* If the command exits with `2`, recheck the command syntax and required options
* Use the `all-spec-types` sample files when you need a simulation recipe that demonstrates every supported operator in one run
* Use the `all-spec-types-fail` recipe with the same spec file when you want to inspect failed spec evaluation output, including `errorCode` values, in simulation mode

---

## Notes

* Uses FakeDevice
* Does not require hardware
* Preserves session artifacts even when validation or runtime errors occur
