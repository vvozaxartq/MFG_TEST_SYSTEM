# Command: device exec

## Description

Send a raw command to `FakeDevice` for quick troubleshooting.

---

## Syntax

```bash
ats device exec --command <text> [--output <directory>]
```

```bash
ats device exec --command <text> [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v,...>]
```

---

## Parameters

* `--command <text>`: command text sent to `FakeDevice`
* `--output <directory>`: optional output directory, defaults to current working directory
* `--output-template <path>`: optional relative or absolute output folder template; supports `%Var%` and `{Var}` tokens such as `{SN}`, `{SessionId}`, `{Station}`, `{Mode}`, and direct date format tokens such as `{yyyyMMdd_HHmmss}`
* `--json-template <path>`: optional result JSON path template, relative to the resolved output directory when not absolute
* `--csv-template <path>`: optional result CSV path template, relative to the resolved output directory when not absolute
* `--log-template <path>`: optional session log path template, relative to the resolved output directory when not absolute
* `--sn <value>`: optional product serial number recorded into the session and available to naming templates
* `--prompt-sn`: prompt for the serial number before the command executes
* `--station <name>`: optional station name recorded into the session and available to naming templates
* `--mode <name>`: optional mode label recorded into the session; defaults to `DEVICE`
* `--vars <k=v,...>`: optional template variables for path expansion; PowerShell users should wrap the value in quotes or use commas instead of bare semicolons

---

## Example

```bash
ats device exec --command PING
```

```bash
ats device exec --command READ_SERIAL --output artifacts/device-debug
```

```bash
ats device exec --command PING --sn SN12345678 --station ST01 --log-template "logs\{SN}_{yyyyMMdd_HHmmss}.log"
```

---

## Expected Output

* console response summary
* result.json
* result.csv
* one dedicated session log file for the session; the default log file name is `session_<SessionId>.log`
* one machine-readable structured event log file for the session; the default file name is `session_<SessionId>.events.jsonl`
* `session.log` uses local time in `yyyy-MM-dd HH:mm:ss.fff` format and shows elapsed runtime plus `item=<command or flow name>` on every line
* `session.log` begins with a session header that includes Session ID, Start Time, Station, Mode, SN, output folder, and environment data
* `session.log` ends with a session result block and a command summary block
* `session.events.jsonl` contains versioned structured log entries with `schemaVersion`, session-global `sequence`, timestamps, entry type, status, and event data for UI/tool/AI replay

---

## Exit Codes

* 0: command executed successfully
* 2: invalid CLI syntax or missing required parameters
* 4: runtime or device execution error

---

## Limitations

* Phase 2 only supports `FakeDevice`
* Unknown commands return a generic `ACK:<command>` response
* The command does not validate recipe or spec files
* If a template path collides with an existing file, the system automatically appends SessionId data so the new run does not overwrite the previous artifacts

---

## Troubleshooting

* If the response is not what you expect, confirm the exact command text
* If a templated artifact path does not look right in PowerShell, wrap brace templates in quotes
* If you need machine-readable execution history, inspect `session.events.jsonl` instead of parsing `session.log`
* If the command exits with `4`, inspect `session.log` for connect, execute, or disconnect errors
