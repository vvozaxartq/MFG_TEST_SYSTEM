# Getting Started

## Build

```bash
dotnet build AutoTestSystem.Next.sln
```

## Run CLI

```bash
dotnet run --project src/ATS.Cli -- test simulate --recipe samples/recipes/demo.recipe.json
```

## Run Thin UI

```bash
dotnet run --project src/ATS.Ui
```

The UI is a thin shell on top of the existing CLI and artifacts. It can:

* run existing ATS CLI commands
* load `result.json`, `session.events.jsonl`, `session.log`, or a session folder
* show session summary, result tree, structured log, and session log views

---

## First Test (Simulation)

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

---

## Phase 2 Test Run

```bash
ats test run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

---

## Multi-Measurement Test Run

```bash
ats test run --recipe samples/recipes/multi-measurement.recipe.json --spec samples/specs/multi-measurement.spec.json
```

This sample shows one script returning multiple fields such as `voltage` and `current`, while step-level `prefix` values turn them into unique `fullKey` values such as `battery.voltage` and `load.current`.

---

## Single Script Debug

```bash
ats script run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json --script ReadSerial
```

---

## Expected Output

* result.json
* result.csv
* session.log
* `result.json` now keeps per-step `MeasurementSet` data with `rawPayload` plus parsed `items`
* each step also keeps `measurements` and `specResults` lists so you can trace which `fullKey` and which rule failed

---

## Exit Codes

* 0: command completed successfully
* 1: test or script execution completed but one or more specs failed
* 2: invalid CLI arguments
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* Core execution remains CLI-first; the current UI is an optional thin shell on top of CLI and artifacts
* All commands use `FakeDevice`; real hardware is not implemented yet
* JSON files are the supported input format for recipe and spec data
* Legacy single-value recipes still work; the runtime automatically wraps them into a one-item `MeasurementSet`
* The current UI is Windows-only and intentionally thin; it does not replace CLI workflows or implement business logic

---

## Troubleshooting

* If the recipe or spec path is wrong, the CLI exits with a clear error and still writes session artifacts when a session was created
* If `result.json` is missing, verify the output directory is writable
* If build output does not exist yet, run `dotnet build AutoTestSystem.Next.sln` first
