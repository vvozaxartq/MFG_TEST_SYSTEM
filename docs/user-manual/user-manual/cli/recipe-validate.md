# Command: recipe validate

## Description

Validate recipe structure, step execution-policy fields, multi-measurement declarations, and recipe-to-spec mapping before test execution. Validation enforces exact `fullKey` matching between declared measurements and `SpecRule.targetKey`.

---

## Syntax

```bash
ats recipe validate --recipe <file> [--spec <file>] [--output <directory>]
```

```bash
ats recipe validate --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v,...>]
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
* `--sn <value>`: optional product serial number recorded into the validation session and available to naming templates
* `--prompt-sn`: prompt for the serial number before validation starts
* `--station <name>`: optional station name recorded into the validation session and available to naming templates
* `--mode <name>`: optional mode label recorded into the validation session; defaults to `VALIDATE`
* `--vars <k=v,...>`: optional template variables for path expansion; PowerShell users should wrap the value in quotes or use commas instead of bare semicolons

---

## Example

```bash
ats recipe validate --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

```bash
ats recipe validate --recipe samples/recipes/variable-system.recipe.json
```

Example step policy snippet:

```json
{
  "name": "ReadVoltage",
  "command": "READ_VOLTAGE",
  "measurementKey": "battery.voltage",
  "unit": "V",
  "retryCount": 1,
  "timeoutMs": 500,
  "continueOnFailure": false
}
```

Example flow-tree snippet:

```json
{
  "flow": {
    "name": "MainFlow",
    "outcomePolicy": "breakOnStepSuccess",
    "nodes": [
      { "type": "step", "step": "ReadGate" },
      {
        "type": "condition",
        "name": "GateOpen",
        "condition": { "type": "dataEquals", "key": "gateValue", "value": "OPEN" },
        "whenTrue": [
          { "type": "step", "step": "RunPathA" }
        ],
        "whenFalse": [
          {
            "type": "sequence",
            "name": "FallbackSequence",
            "nodes": [
              { "type": "step", "step": "RunPathB" }
            ]
          }
        ]
      }
    ]
  }
}
```

Example repeat-until snippet:

```json
{
  "flow": {
    "name": "MainFlow",
    "nodes": [
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
    ]
  }
}
```

---

Example multi-measurement step:

```json
{
  "name": "BatterySnapshot",
  "command": "READ_BATTERY",
  "prefix": "battery",
  "measurements": [
    { "key": "voltage", "unit": "V" },
    { "key": "current", "unit": "A" }
  ]
}
```

Compatibility notes:

* legacy `measurementKey` / `specKey` steps still validate
* legacy single-value steps are auto-wrapped into a one-item `MeasurementSet` at runtime
* `prefix` should be configured on the recipe step so reused script shapes can emit different `fullKey` values

---

## Expected Output

* validation summary on console
* result.json
* result.csv
* one dedicated session log file for the session; the default log file name is `session_<SessionId>.log`
* one machine-readable structured event log file for the session; the default file name is `session_<SessionId>.events.jsonl`
* `session.log` uses local time in `yyyy-MM-dd HH:mm:ss.fff` format and shows elapsed runtime plus `item=<command or flow name>` on every line
* `session.log` begins with a session header that includes Session ID, Start Time, Station, Mode, SN, output folder, and environment data
* `session.log` ends with a session result block and a validation summary block
* `session.events.jsonl` contains versioned structured log entries with `schemaVersion`, session-global `sequence`, timestamps, entry type, status, and event data for UI/tool/AI replay

---

## Exit Codes

* 0: recipe validation passed
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed

---

## Limitations

* Validation checks JSON structure, required fields, duplicate names/keys, and spec references
* Validation checks `retryCount >= 0` and `timeoutMs >= 0`
* Validation checks multi-measurement declarations, duplicate per-step keys, and duplicate cross-step `fullKey` collisions after prefix expansion
* Validation checks optional `flow` tree nodes, including valid step references, supported condition types, supported `sequence.outcomePolicy` and `repeatUntil.outcomePolicy` values, and `repeatUntil.maxIterations > 0`
* Validation checks `${varName}` placeholder syntax in recipe command, simulated response, and measurement string template fields
* Validation accepts supported `dut.*` placeholders as runtime-provided values and reports them as warnings instead of resolving them
* Validation does not run device commands
* Only JSON recipe and spec files are supported
* Validation rejects duplicate measurement `fullKey` values within a step and across recipe steps
* Validation does not do fuzzy matching from `key` to prefixed `fullKey`
* Validation does not guarantee that runtime-only values such as CLI-supplied variables exist; missing runtime variables still fail during execution
* Supported flow condition types are limited to `previousStepStatus`, `dataExists`, and `dataEquals`, including the `until` condition inside `repeatUntil`
* Supported `sequence.outcomePolicy` and `repeatUntil.outcomePolicy` values are `breakOnStepFailure` and `breakOnStepSuccess`
* If a template path collides with an existing file, the system automatically appends SessionId data so the new run does not overwrite the previous artifacts

---

## Troubleshooting

* If validation reports missing specs, either add inline specs or pass `--spec`
* If validation reports duplicate script names, rename scripts to unique values
* If validation reports duplicate measurement `fullKey` values, adjust the step prefix, recipe prefix, or measurement key so each declared measurement is unique
* If validation reports a missing `targetKey`, update the spec rule to the exact measurement `fullKey`
* If validation reports malformed variable placeholder syntax, fix the `${varName}` token before running the recipe
* If validation reports invalid `retryCount` or `timeoutMs`, update the recipe step policy so those numeric values are zero or positive
* If validation reports an unknown flow step reference, make sure every `flow` step node points to a script name declared in `scripts`
* If validation reports an unsupported flow condition type, change it to `previousStepStatus`, `dataExists`, or `dataEquals`
* If validation reports an invalid `repeatUntil` node, verify that `nodes` is not empty and `maxIterations` is greater than zero
* If validation reports an unsupported `sequence.outcomePolicy`, change it to `breakOnStepFailure`, `breakOnStepSuccess`, or remove it to keep the default behavior
* If validation reports an unsupported `repeatUntil.outcomePolicy`, change it to `breakOnStepFailure`, `breakOnStepSuccess`, or remove it to keep the default behavior
* If validation reports a `dut.*` warning, supply the value at execution time through `--sn`, `--station`, or `--vars DutId=...`, `DutIndex=...`, `Slot=...`
* If a templated artifact path does not look right in PowerShell, wrap brace templates in quotes
* If you need machine-readable validation history, inspect `session.events.jsonl` instead of parsing `session.log`
* Inspect `result.csv` for a compact error list when multiple issues are reported
