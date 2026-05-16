# Command: script run

## Description

Run one script from a recipe for targeted debugging. The selected step can still emit multiple measurements and evaluate multiple rules by exact `fullKey`, using the step prefix first and the recipe prefix as fallback.

---

## Syntax

```bash
ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>]
```

```bash
ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v,...>]
```

---

## Parameters

* `--recipe <file>`: path to recipe JSON file
* `--script <name>`: script name to run from the recipe
* `--spec <file>`: optional external spec JSON file; if omitted, inline recipe specs are used
* `--output <directory>`: optional output directory, defaults to current working directory
* `--output-template <path>`: optional relative or absolute output folder template; supports `%Var%` and `{Var}` tokens such as `{SN}`, `{SessionId}`, `{Recipe}`, `{Station}`, `{Mode}`, and direct date format tokens such as `{yyyyMMdd_HHmmss}`
* `--json-template <path>`: optional result JSON path template, relative to the resolved output directory when not absolute
* `--csv-template <path>`: optional result CSV path template, relative to the resolved output directory when not absolute
* `--log-template <path>`: optional session log path template, relative to the resolved output directory when not absolute
* `--sn <value>`: optional product serial number recorded into the session and available to naming templates
* `--prompt-sn`: prompt for the serial number before the script run starts
* `--station <name>`: optional station name recorded into the session and available to naming templates
* `--mode <name>`: optional mode label recorded into the session; defaults to `SCRIPT`
* `--vars <k=v,...>`: optional template variables for path expansion; PowerShell users should wrap the value in quotes or use commas instead of bare semicolons

---

## Recipe Variables

Execution-time recipe variables use `${varName}` with the fixed precedence:

1. Step
2. DUT
3. Global

`script run` resolves `${varName}` in the selected step before it executes, including command text, simulated response, and measurement string fields such as `sourcePath`, `unit`, and `description`.

Canonical DUT variables are:

* `${dut.id}`
* `${dut.index}`
* `${dut.sn}`
* `${dut.station}`
* `${dut.slot}`
* `${dut.isSimulated}`

`dut.*` placeholders resolve only from `DutContext`; Step and Global variables do not override them.

Use `--sn`, `--station`, and `--vars "DutId=...,DutIndex=...,Slot=..."` to feed DUT values for the selected script. `dut.id` falls back to the active serial number when `DutId` is omitted.

---

## Step Execution Policy

The selected step can still use execution-policy fields from the recipe:

* `retryCount`: retry the selected step after an execution error or timeout
* `timeoutMs`: cancel the selected step attempt when execution exceeds the configured timeout
* `continueOnFailure`: accepted for recipe compatibility, but it has no practical effect in `script run` because only one step is executed

---

## Flow Tree Compatibility

If a recipe defines a `flow` tree, `script run` still executes the selected script directly by name.

This command does not walk sequence containers, condition branches, or `repeatUntil` loops around that step. It is intentionally a targeted single-step debugging path.

---

## Example

```bash
ats script run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json --script ReadSerial
```

```bash
ats script run --recipe samples/recipes/multi-measurement.recipe.json --spec samples/specs/multi-measurement.spec.json --script BatterySnapshot
```

```bash
ats script run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --script ReadDeviceInfo
```

```bash
ats script run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --script ReadDeviceInfo --output .codex-out --output-template script-runs\%ProductSN%\%CurTime% --log-template logs\%ProductSN%_%CurTime%.log --vars "ProductSN=SN001,LoginUser=MTE"
```

```bash
ats script run --recipe samples/recipes/all-spec-types.recipe.json --spec samples/specs/all-spec-types.spec.json --script ReadDeviceInfo --sn SN12345678 --station ST01 --log-template "logs\{SN}_{yyyyMMdd_HHmmss}.log"
```

```bash
ats script run --recipe samples/recipes/variable-system.recipe.json --script ReadSnapshot --sn SN-SCRIPT-001
```

```bash
ats script run --recipe samples/recipes/variable-system.recipe.json --script ReadSnapshot --sn SN-SCRIPT-001 --station ST01 --vars "DutId=DUT-SCRIPT-01,DutIndex=0,Slot=RACK01"
```

---

## Expected Output

* one `result.json` file for the session
* one `result.csv` file for the session
* one dedicated session log file for the session; the default log file name is `session_<SessionId>.log`
* one machine-readable structured event log file for the session; the default file name is `session_<SessionId>.events.jsonl`
* Measurement records include `key`, `prefix`, and `fullKey`
* the selected step includes `measurementSet` with `rawPayload` and parsed `items`
* the selected step also includes `measurements` and `specResults` lists so multiple field-level rule results stay visible during single-step debugging
* Spec evaluation records show the exact `targetKey` fullKey used for matching
* `result.json` includes `FlowResultTree`; for `script run` this remains a single-step execution path rather than a walked container/branch/repeat tree
* Console output keeps measurements on separate lines and prints a compact rule summary as `[PASS]/[FAIL] rule | targetKey | spec | code`
* `session.log` uses local time in `yyyy-MM-dd HH:mm:ss.fff` format and shows elapsed runtime plus `item=<step or flow name>` on every line
* `session.log` begins with a session header that includes Session ID, Start Time, Recipe file, Station, Mode, SN, output folder, and environment data
* `session.log` ends with a session result block and a `Test Summary` table containing item name, result, test time, elapsed runtime, retry count, and data summary
* `session.events.jsonl` contains versioned structured log entries with `schemaVersion`, session-global `sequence`, timestamps, entry type, status, and event data for UI/tool/AI replay
* Variable-based script runs also emit `VariableResolved` and `VariableResolutionFailed` events in `session.events.jsonl`
* `VariableResolved` entries include `requestedName`, `resolvedName`, `scope`, `value`, and `source` for canonical DUT traceability

---

## Exit Codes

* 0: selected script passed spec evaluation
* 1: selected script completed but failed spec evaluation
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* The script name must match a script in the recipe
* Phase 2 uses `FakeDevice`; real hardware is not implemented yet
* Only one script can be executed per command
* Spec rules for the selected script must reference the exact declared measurement `fullKey`
* If the selected script shape is reused elsewhere in the recipe, use step or recipe prefixes to avoid `fullKey` collisions
* Missing execution-time `${varName}` variables fail the selected step; there is no silent fallback or recursive expansion
* `retryCount` only applies to execution errors and timeouts; it does not retry spec-evaluation failures
* If the recipe defines a `flow` tree, `script run` does not execute container/branch logic around the selected step
* `ats recipe validate` keeps supported `dut.*` placeholders unresolved and reports them as runtime-provided warnings
* If a template path collides with an existing file, the system automatically appends SessionId data so the new run does not overwrite the previous artifacts
* `DataCollection` uses `last-write-wins` if the same `fullKey` is written again; use `session.events.jsonl` to inspect every write event

---

## Troubleshooting

* If the command reports `Script '<name>' was not found`, verify the script name in the recipe file
* If the command exits with `3`, validate the recipe/spec pair before re-running
* If the selected script uses prefixes, confirm the spec rule `targetKey` matches the emitted `fullKey` exactly
* If a templated artifact path does not look right in PowerShell, wrap brace templates in quotes
* If the step fails before sending a command because a `dut.*` variable is missing, inspect `session.events.jsonl` and verify the value was supplied through `--sn`, `--station`, or `--vars DutId=...`, `DutIndex=...`, `Slot=...`
* If the step fails before sending a command because a non-DUT variable is missing, inspect `session.events.jsonl` and verify the value exists in Step or Global scope
* If the selected step times out or retries, inspect `session.events.jsonl` for `StepTimedOut` and `StepRetried`, then compare the recorded retry count with the recipe's `retryCount`
* If you need machine-readable step or measurement history, inspect `session.events.jsonl` instead of parsing `session.log`
* If the selected script returns unexpected data, inspect `session.log` and `result.json`
