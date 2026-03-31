# Command: test simulate

## Description

Run test flow in simulation mode without real hardware.

---

## Syntax

```bash
ats test simulate --recipe <file> [--spec <file>] [--output <directory>]
```

---

## Parameters

* `--recipe <file>`: path to recipe JSON file
* `--spec <file>`: optional external spec JSON file; if omitted, inline recipe specs are used
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Example

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json --output artifacts/demo-run
```

```bash
ats test simulate --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

---

## Expected Output

* result.json
* result.csv
* session.log

---

## Exit Codes

* 0: all script results passed spec evaluation
* 1: one or more script results failed spec evaluation
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* Only JSON recipe and spec files are supported
* Only `FakeDevice` is available; no real hardware integration is included

---

## Troubleshooting

* If the command reports `Recipe file was not found`, verify the `--recipe` path
* If the command reports validation errors, verify the recipe/spec mapping and supported operators
* If output files are missing, verify the `--output` directory is writable
* If the command exits with `2`, recheck the command syntax and required options

---

## Notes

* Uses FakeDevice
* Does not require hardware
* Preserves session artifacts even when validation or runtime errors occur
