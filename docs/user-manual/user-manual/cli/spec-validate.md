# Command: spec validate

## Description

Validate spec definitions and supported operators before test execution. The current preferred schema is `rules[]`, where each rule `targetKey` uses the exact measurement `fullKey`, such as `battery.voltage`.

---

## Syntax

```bash
ats spec validate --spec <file> [--output <directory>]
```

```bash
ats spec validate --spec <file> [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v,...>]
```

---

## Parameters

* `--spec <file>`: path to spec JSON file
* `--output <directory>`: optional output directory, defaults to current working directory
* `--output-template <path>`: optional relative or absolute output folder template; supports `%Var%` and `{Var}` tokens such as `{SN}`, `{SessionId}`, `{Station}`, `{Mode}`, and direct date format tokens such as `{yyyyMMdd_HHmmss}`
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
ats spec validate --spec samples/specs/phase2.spec.json
```

```bash
ats spec validate --spec samples/specs/multi-measurement.spec.json
```

---

Example rule:

```json
{
  "name": "Battery Voltage Range",
  "targetKey": "battery.voltage",
  "ruleType": "Range",
  "min": 11.5,
  "max": 12.8,
  "errorCode": "BATTERY_VOLTAGE_RANGE",
  "message": "Battery voltage out of range"
}
```

Compatibility notes:

* `rules[]` is the preferred multi-measurement format
* legacy `specs[]` documents still validate for older single-value flows

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

* 0: spec validation passed
* 2: invalid CLI syntax or missing required parameters
* 3: spec validation failed

---

## Supported Operators

* `Equal`
* `Range`
* `GreaterThan`
* `LessThan`
* `Regex`
* `Contain`
* `NotEqual`
* `Bypass`

---

## Limitations

* Numeric operators require numeric values in the spec and the actual response
* Only JSON spec files are supported
* Validation checks definitions only; it does not run any device commands
* `spec validate` checks rule structure, but recipe-to-`fullKey` mapping is verified by `ats recipe validate`, `ats test run`, or `ats test simulate`
* `spec validate` does not know recipe prefixes by itself, so it cannot prove that a `targetKey` exists until the spec is checked together with a recipe
* If a template path collides with an existing file, the system automatically appends SessionId data so the new run does not overwrite the previous artifacts

---

## Troubleshooting

* If validation reports an unsupported operator, verify the operator spelling
* If validation reports missing numeric values, check `expected`, `minimum`, or `maximum` fields
* If a later recipe validation reports an unknown `targetKey`, update the spec rule to the exact measurement `fullKey` emitted by the recipe step
* If a templated artifact path does not look right in PowerShell, wrap brace templates in quotes
* If you need machine-readable validation history, inspect `session.events.jsonl` instead of parsing `session.log`
* Use `result.json` or `result.csv` to review all reported validation issues
