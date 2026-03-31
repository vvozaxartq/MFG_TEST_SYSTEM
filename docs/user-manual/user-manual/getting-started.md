# Getting Started

## Build

```bash
dotnet build AutoTestSystem.Next.sln
```

## Run CLI

```bash
dotnet run --project src/ATS.Cli -- test simulate --recipe samples/recipes/demo.recipe.json
```

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

## Single Script Debug

```bash
ats script run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json --script ReadSerial
```

---

## Expected Output

* result.json
* result.csv
* session.log

---

## Exit Codes

* 0: command completed successfully
* 1: test or script execution completed but one or more specs failed
* 2: invalid CLI arguments
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* Phase 2 remains CLI-first and does not include UI
* All commands use `FakeDevice`; real hardware is not implemented yet
* JSON files are the supported input format for recipe and spec data

---

## Troubleshooting

* If the recipe or spec path is wrong, the CLI exits with a clear error and still writes session artifacts when a session was created
* If `result.json` is missing, verify the output directory is writable
* If build output does not exist yet, run `dotnet build AutoTestSystem.Next.sln` first
