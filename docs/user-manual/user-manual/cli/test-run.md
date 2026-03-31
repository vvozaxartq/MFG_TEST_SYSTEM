# Command: test run

## Description

Run a full CLI test session using recipe and spec data.

---

## Syntax

```bash
ats test run --recipe <file> [--spec <file>] [--output <directory>]
```

---

## Parameters

* `--recipe <file>`: path to recipe JSON file
* `--spec <file>`: optional external spec JSON file; if omitted, inline recipe specs are used
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Example

```bash
ats test run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

```bash
ats test run --recipe samples/recipes/demo.recipe.json --output artifacts/test-run
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

* Phase 2 uses `FakeDevice`; real hardware is not implemented yet
* Only JSON recipe and spec files are supported
* Runtime errors stop the active run after the failing script, but completed script results remain in the artifacts

---

## Troubleshooting

* If the command reports `No spec definitions were found`, add inline recipe specs or pass `--spec`
* If a script reports `Error`, inspect `session.log` and `result.json` for the failing command and message
* If the command exits with `3`, run `ats recipe validate` or `ats spec validate` first
