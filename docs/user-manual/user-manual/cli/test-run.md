# Command: test run

## Description

Run a full CLI test session using recipe and spec data. One step can emit multiple measurements, and one step can evaluate multiple rules. Measurements are tracked by `fullKey`, where a step prefix overrides the recipe prefix and empty prefix keeps `fullKey` equal to the original `key`.

---

## Syntax

```bash
ats test run --recipe <file> [--spec <file>] [--output <directory>]
```

```bash
ats test run --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v,...>]
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
* `--sn <value>`: optional product serial number; when supplied, the value is recorded in the session and can be used by naming templates
* `--prompt-sn`: prompt for the serial number before the run starts; useful for scanner-style operator flow
* `--station <name>`: optional station name recorded in the session and available to naming templates
* `--mode <name>`: optional mode label recorded in the session; defaults to `RUN`
* `--vars <k=v,...>`: optional template variables for path expansion; PowerShell users should wrap the value in quotes or use commas instead of bare semicolons

---

## Multi-Measurement Recipe Shape

Use step-level `prefix` and `measurements[]` when one command returns multiple fields:

```json
{
  "scripts": [
    {
      "name": "BatterySnapshot",
      "command": "READ_BATTERY",
      "prefix": "battery",
      "measurements": [
        { "key": "voltage", "unit": "V" },
        { "key": "current", "unit": "A" }
      ],
      "simulatedResponse": "{\"voltage\":12.3,\"current\":1.4}"
    }
  ]
}
```

Matching spec rules use exact `targetKey` values:

```json
{
  "rules": [
    { "name": "Battery Voltage Range", "targetKey": "battery.voltage", "ruleType": "Range", "min": 11.5, "max": 12.8 },
    { "name": "Battery Current Limit", "targetKey": "battery.current", "ruleType": "LessThan", "expected": "1.8" }
  ]
}
```

Compatibility notes:

* old single-value `measurementKey` recipes still work
* old single-value steps are auto-wrapped into a one-item `MeasurementSet`
* `prefix` belongs to the recipe step; the script/device layer keeps the raw keys

---

## Recipe Variables

Execution-time recipe variables use `${varName}` and resolve in the fixed order:

1. Step variables
2. DUT variables
3. Global variables

`test run` supports `${varName}` in:

* step `command`
* step `simulatedResponse`
* measurement `sourcePath`
* measurement `unit`
* measurement `description`

Canonical DUT variables are:

* `${dut.id}`
* `${dut.index}`
* `${dut.sn}`
* `${dut.station}`
* `${dut.slot}`
* `${dut.isSimulated}`

`dut.*` placeholders resolve only from `DutContext`; Step and Global variables do not override them.

Current CLI input mapping is:

* `--sn` -> `dut.sn`
* `--station` -> `dut.station`
* `--vars DutId=...`, `DutIndex=...`, `Slot=...` -> `dut.id`, `dut.index`, `dut.slot`
* `dut.id` falls back to the active serial number when `DutId` is not supplied
* `dut.isSimulated` is `false` for `test run`

Example recipe entry:

```json
{
  "variables": {
    "VoltagePath": "payload.globalVoltage"
  },
  "scripts": [
    {
      "name": "ReadSnapshot",
      "command": "READ_${SN}",
      "variables": {
        "VoltagePath": "payload.stepVoltage"
      },
      "measurements": [
        {
          "key": "voltage",
          "sourcePath": "${VoltagePath}"
        }
      ]
    }
  ]
}
```

`--vars`, `--sn`, `--station`, and `--mode` feed the Global scope for execution-time variables.

Artifact naming templates such as `%SN%` and `{SN}` are separate from `${SN}` execution-time variables.

---

## Step Execution Policy

Each recipe step can optionally declare execution-policy fields that control deterministic flow behavior:

* `retryCount`: retry the step this many additional times after an execution error or timeout
* `timeoutMs`: cancel the active step attempt when command execution exceeds the configured timeout in milliseconds
* `continueOnFailure`: continue to the next step after the final execution error instead of stopping the whole flow immediately

Example recipe step:

```json
{
  "name": "ReadVoltage",
  "command": "READ_VOLTAGE",
  "measurementKey": "battery.voltage",
  "unit": "V",
  "retryCount": 1,
  "timeoutMs": 500,
  "continueOnFailure": false,
  "simulatedResponse": "12.3"
}
```

Behavior notes:

* Spec failures still produce `Failed` step results without using retry
* Execution errors and timeouts produce `Error` step results and may retry when `retryCount > 0`
* `continueOnFailure` applies only after the final failed attempt

---

## Flow Tree Foundation

Recipes can now keep `scripts` as the step catalog while optionally adding a `flow` tree to control execution order.

Supported node types:

* `step`: run one named script from the `scripts` collection
* `sequence`: run child nodes in order and optionally stop early through a minimal `outcomePolicy`
* `condition`: choose `whenTrue` or `whenFalse` deterministically
* `repeatUntil`: run child nodes, then evaluate `until` after each iteration

`sequence` fields:

* `nodes`: child nodes to execute in declaration order
* `outcomePolicy`: optional container-level early-termination policy; supported values are `breakOnStepFailure` and `breakOnStepSuccess`

Supported condition types in this phase:

* `previousStepStatus`: compare the most recently completed step status against `condition.status`
* `dataExists`: check whether `DataCollection` contains `condition.key`
* `dataEquals`: check whether `DataCollection[condition.key]` equals `condition.value`

`repeatUntil` fields:

* `nodes`: child nodes to execute each iteration
* `until`: narrow deterministic stop condition
* `outcomePolicy`: optional early-termination policy; supported values are `breakOnStepFailure` and `breakOnStepSuccess`
* `maxIterations`: maximum number of iterations to allow; must be greater than zero
* `failOnMaxIterations`: when `true`, stop the run with a deterministic error if the stop condition is still false at the final iteration

Example repeat fragment:

```json
{
  "type": "repeatUntil",
  "name": "WaitForReady",
  "outcomePolicy": "breakOnStepFailure",
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
  "outcomePolicy": "breakOnStepSuccess",
  "nodes": [
    { "type": "step", "step": "ReadGate" },
    { "type": "step", "step": "ReadAfterGate" }
  ]
}
```

Example flow-tree recipe fragment:

```json
{
  "scripts": [
    { "name": "ReadGate", "command": "READ_GATE", "measurementKey": "gateValue", "simulatedResponse": "OPEN" },
    { "name": "RunPathA", "command": "RUN_A", "measurementKey": "pathAValue", "simulatedResponse": "A" },
    { "name": "RunPathB", "command": "RUN_B", "measurementKey": "pathBValue", "simulatedResponse": "B" }
  ],
  "flow": {
    "name": "MainFlow",
    "nodes": [
      { "type": "step", "step": "ReadGate" },
      {
        "type": "condition",
        "name": "GateOpen",
        "condition": { "type": "dataEquals", "key": "gateValue", "value": "OPEN" },
        "whenTrue": [
          {
            "type": "sequence",
            "name": "OpenSequence",
            "nodes": [
              { "type": "step", "step": "RunPathA" }
            ]
          }
        ],
        "whenFalse": [
          { "type": "step", "step": "RunPathB" }
        ]
      }
    ]
  }
}
```

If `flow` is omitted, `test run` keeps the legacy flat sequential behavior and runs `scripts` in declaration order.

---

## Example

```bash
ats test run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

```bash
ats test run --recipe samples/recipes/demo.recipe.json --output artifacts/test-run
```

```bash
ats test run --recipe samples/recipes/multi-measurement.recipe.json --spec samples/specs/multi-measurement.spec.json
```

```bash
ats test run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json
```

```bash
ats test run --recipe samples/recipes/all-spec-types-fail.recipe.json --spec samples/specs/all-spec-types.spec.json --output artifacts/all-spec-types-fail
```

```bash
ats test run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --output .codex-out --output-template runs\%ProductSN%\%CurTime% --json-template json\%ProductSN%_%CurTime%.json --csv-template csv\%ProductSN%_%CurTime%.csv --log-template logs\%ProductSN%_%CurTime%.log --vars "ProductSN=SN001,LoginUser=MTE"
```

```bash
ats test run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --output .codex-out\session-run --sn SN90001 --station ST01 --log-template "logs\{SN}_{yyyyMMdd_HHmmss}.log" --json-template "json\{Recipe}_{SN}_{SessionId}.json"
```

```bash
ats test run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --prompt-sn --output .codex-out\session-run
```

```bash
ats test run --recipe samples/recipes/variable-system.recipe.json --sn SN-VAR-001 --output artifacts/variable-run
```

```bash
ats test run --recipe samples/recipes/variable-system.recipe.json --sn SN90001 --station ST01 --vars "DutId=DUT-A01,DutIndex=0,Slot=SLOT01"
```

---

## Expected Output

* one `result.json` file for the session
* one `result.csv` file for the session
* one dedicated session log file for the session; the default log file name is `session_<SessionId>.log`
* one machine-readable structured event log file for the session; the default file name is `session_<SessionId>.events.jsonl`
* `result.json` and `result.csv` include measurement `key`, `prefix`, and `fullKey`
* each step in `result.json` now includes `measurementSet` with `source`, `command`, `collectedAt`, `rawPayload`, and parsed `items`
* each step also includes `measurements` and `specResults` lists so one command can be traced to multiple field-level and rule-level results
* `result.json` also includes `SessionInfo` as the canonical source for session metadata, input values, final status, and artifact paths
* `result.json` now also includes `FlowResultTree`, which preserves sequence containers, selected condition branches, repeat iteration groups, node status, timestamps, and `CountsTowardFinalStatus`
* `FlowResultTree` sequence nodes can now record `OutcomePolicy`, `StopReason`, and `TriggeredByNodeName` when a container terminates early
* `FlowResultTree` repeat nodes now use explicit stop reasons such as `ConditionSatisfied`, `MaxIterationsReached`, `IterationFailure`, `StepFailureBreak`, and `StepSuccessBreak`
* Spec evaluation records use exact `targetKey` values such as `battery.voltage`
* Console output keeps measurements on separate lines and prints a compact rule summary as `[PASS]/[FAIL] rule | targetKey | spec | code`
* `session.log` uses local time in `yyyy-MM-dd HH:mm:ss.fff` format and shows elapsed runtime plus `item=<step or flow name>` on every line
* `session.log` begins with a session header that includes Session ID, Start Time, Recipe file, Station, Mode, SN, output folder, and environment data
* `session.log` ends with a session result block and a `Test Summary` table containing item name, result, test time, elapsed runtime, retry count, and data summary
* `session.events.jsonl` contains versioned structured log entries with `schemaVersion`, session-global `sequence`, timestamps, entry type, status, and event data for UI/tool/AI replay
* `session.events.jsonl` records `StepRetried` and `StepTimedOut` events when a step policy triggers retry or timeout handling
* `session.events.jsonl` records `ContainerStarted` and `ContainerCompleted` for explicit flow sequences plus `BranchEvaluated` and `BranchSelected` for condition nodes
* `ContainerStarted` and `ContainerCompleted` entries now include sequence `outcomePolicy`, `stopReason`, and `triggeredByNodeName` when a sequence container terminates early
* `session.events.jsonl` records `LoopStarted`, `LoopIterationStarted`, `LoopIterationCompleted`, `LoopConditionEvaluated`, `LoopMaxIterationsReached`, and `LoopCompleted` when `repeatUntil` nodes are used
* If `${varName}` is used, `session.events.jsonl` also records `VariableResolved` and `VariableResolutionFailed` events with scope and source data
* `VariableResolved` entries include `requestedName`, `resolvedName`, `scope`, `value`, and `source` so tooling can distinguish canonical DUT names from the original placeholder text
* Non-terminal failed loop iterations remain visible in `result.json`, `result.csv`, `session.log`, and `session.events.jsonl`, but a later successful iteration can still produce an overall passing run

---

## Exit Codes

* 0: all script results passed spec evaluation
* 1: one or more script results failed spec evaluation
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* Phase 2 uses `FakeDevice`; real hardware is not implemented yet
* Only JSON recipe and spec files are supported
* Runtime errors stop the active run by default, but `continueOnFailure` can keep later steps running after the final failed attempt
* `SpecRule.targetKey` must exactly match a declared measurement `fullKey`; the system does not auto-map `voltage` to `battery.voltage`
* When the same raw key is reused in multiple steps, you must use different step or recipe prefixes if you want both results to coexist without collision
* Unknown template variables resolve to `NA`
* Missing execution-time `${varName}` variables fail the run; there is no silent fallback or recursive expansion
* `retryCount` only applies to execution errors and timeouts; it does not retry spec-evaluation failures
* Flow conditions are intentionally narrow in this phase; `repeatUntil` does not add a general-purpose expression language, `while` syntax, or parallel loop execution
* `sequence.outcomePolicy` and `repeatUntil.outcomePolicy` only support `breakOnStepFailure` and `breakOnStepSuccess` in this phase
* `ats recipe validate` does not resolve runtime DUT values; it reports supported `dut.*` placeholders as warnings and leaves value checks to execution time
* If a template path collides with an existing file, the system automatically appends SessionId data so the new run does not overwrite the previous artifacts
* `DataCollection` uses `last-write-wins` if the same `fullKey` is written again; use `session.events.jsonl` to inspect every write event

---

## Troubleshooting

* If the command reports `No spec definitions were found`, add inline recipe specs or pass `--spec`
* If validation reports duplicate measurement `fullKey` values, update recipe prefixes or measurement keys so every declared measurement remains unique
* If validation reports a missing `targetKey`, update the spec rule to the exact `fullKey` shown in `result.json`, `result.csv`, or `session.log`
* If a templated artifact path does not look right in PowerShell, wrap brace templates in quotes and wrap `--vars` in quotes or use comma-separated assignments such as `"LoginUser=MTE"`
* If a run fails because a `dut.*` variable is missing, inspect `session.events.jsonl` for `VariableResolutionFailed` and verify the value was supplied through `--sn`, `--station`, or `--vars DutId=...`, `DutIndex=...`, `Slot=...`
* If a run fails because a non-DUT variable is missing, inspect `session.events.jsonl` for `VariableResolutionFailed` and verify the value exists in Step or Global scope
* If a step times out, inspect `session.events.jsonl` for `StepTimedOut`, verify the configured `timeoutMs`, and confirm the command/device path can finish within that budget
* If a step retried unexpectedly, inspect `session.events.jsonl` for `StepRetried` and compare the retry count in the `Test Summary` table with the step's configured `retryCount`
* If a branch did not go where you expected, inspect `session.events.jsonl` for `BranchEvaluated` and `BranchSelected` to see the condition type, actual value, and selected path
* If a repeat loop does not stop when expected, inspect `LoopConditionEvaluated` in `session.events.jsonl` and verify the runtime `previousStepStatus`, `condition.key`, or `condition.value`
* If a repeat loop stops with a runtime error, inspect `LoopMaxIterationsReached` and confirm whether `maxIterations` and `failOnMaxIterations` match the intended polling behavior
* If a repeat loop stops earlier than the `until` condition suggests, inspect `FlowResultTree.StopReason` and verify whether `outcomePolicy` intentionally terminated on a passed or failed iteration
* If a sequence container stops earlier than expected, inspect `FlowResultTree.StopReason`, `FlowResultTree.OutcomePolicy`, and `FlowResultTree.TriggeredByNodeName` to see which child node caused the early break
* If you need to understand which container, branch, or repeat iteration produced a step result, inspect `FlowResultTree` in `result.json` before falling back to raw log replay
* If a nested flow stops earlier than expected, inspect `session.events.jsonl` for `ContainerCompleted` and compare the final container status with the child step results
* If `--prompt-sn` appears to wait forever, verify your terminal can provide standard input and that your barcode scanner sends a trailing Enter key
* If a script reports `Error`, inspect `session.log` and `result.json` for the failing command and message
* If you need machine-readable step or measurement history, inspect `session.events.jsonl` instead of parsing `session.log`
* If the command exits with `3`, run `ats recipe validate` or `ats spec validate` first
* Use `samples/recipes/all-spec-types.recipe.json` with `samples/specs/all-spec-types.spec.json` when you want a single runnable sample that demonstrates every supported spec rule type
* Use `samples/recipes/all-spec-types-fail.recipe.json` with `samples/specs/all-spec-types.spec.json` when you want a runnable sample that intentionally produces multiple `Failed` spec results and non-empty `errorCode` values
