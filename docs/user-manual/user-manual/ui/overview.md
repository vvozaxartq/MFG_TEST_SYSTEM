# Thin UI Overview

## Purpose

`ATS.Ui` is a thin Windows UI layer on top of the existing ATS CLI and artifact outputs.

It is intended for:

* running existing CLI commands from a simple desktop shell
* loading completed session artifacts without manually opening files
* reviewing result trees, structured logs, and human-readable session logs

It is not intended to replace CLI or move business logic into the UI.

---

## Launch

```bash
dotnet run --project src/ATS.Ui
```

After building, the UI can also be started from:

```bash
src\ATS.Ui\bin\Debug\net9.0-windows\ATS.Ui.exe
```

---

## Current Features

* CLI runner panel for:
  * `test simulate`
  * `test run`
  * `script run`
  * `device exec`
  * `recipe validate`
  * `spec validate`
  * custom ATS command text
* Artifact loader that accepts:
  * session folder
  * `result.json`
  * `session.events.jsonl`
  * `session.log`
* Session summary view
* Result tree view for test/device/validation artifacts
* Structured log grid with event-type and text filtering
* Session log text viewer
* Quick-open buttons for the loaded artifact files

---

## Data Sources

The UI only consumes existing outputs and models from Core/Application:

* `result.json`
* `session.events.jsonl`
* `session.log`
* existing CLI command output

The UI does not implement recipe execution, spec evaluation, variable resolution, or flow logic on its own.

---

## Expected Behavior

* Running a command from the UI still executes the ATS CLI under the hood
* When the CLI prints artifact paths, the UI auto-loads the resulting `result.json`
* Loading a session folder or artifact file refreshes:
  * summary
  * result tree
  * structured log grid
  * session log viewer

---

## Limitations

* Windows-only because the current thin UI is implemented as a Windows desktop app
* The UI is intentionally thin and does not replace CLI-first workflows
* No recipe editor, mapping editor, multi-DUT dashboard, or lock monitor is included
* The current UI reflects current single-session artifact models; future multi-DUT work will require UI expansion
* The UI is designed to read the current ATS artifact shapes; unsupported custom JSON files may not load correctly

---

## Troubleshooting

* If the UI cannot find the repo root, run it from the checked-out repository build output rather than copying the executable elsewhere
* If a CLI run fails, inspect the embedded CLI output panel first and then open `session.log` or `session.events.jsonl`
* If loading a session folder shows incomplete data, verify that `result.json` and related artifacts were actually generated for that session
* If the structured log grid is empty, verify that `session.events.jsonl` exists and is not blank
* If opening an artifact file fails, verify the file still exists at the loaded path
