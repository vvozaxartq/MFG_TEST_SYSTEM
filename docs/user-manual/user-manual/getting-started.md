# Getting Started

## Build

```bash
dotnet build
```

## Run CLI

```bash
dotnet run --project src/ATS.Cli
```

---

## First Test (Simulation)

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

---

## Expected Output

* result.json
* result.csv
* session.log

---

## Notes

Simulation mode uses FakeDevice and does not require real hardware.
