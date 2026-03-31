# Command: device exec

## Description

Send a raw command to `FakeDevice` for quick troubleshooting.

---

## Syntax

```bash
ats device exec --command <text> [--output <directory>]
```

---

## Parameters

* `--command <text>`: command text sent to `FakeDevice`
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Example

```bash
ats device exec --command PING
```

```bash
ats device exec --command READ_SERIAL --output artifacts/device-debug
```

---

## Expected Output

* console response summary
* result.json
* result.csv
* session.log

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

---

## Troubleshooting

* If the response is not what you expect, confirm the exact command text
* If the command exits with `4`, inspect `session.log` for connect, execute, or disconnect errors
