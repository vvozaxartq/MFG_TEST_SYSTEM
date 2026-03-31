# Command: script run

## Description

Run one script from a recipe for targeted debugging.

---

## Syntax

```bash
ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>]
```

---

## Parameters

* `--recipe <file>`: path to recipe JSON file
* `--script <name>`: script name to run from the recipe
* `--spec <file>`: optional external spec JSON file; if omitted, inline recipe specs are used
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Example

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

* 0: selected script passed spec evaluation
* 1: selected script completed but failed spec evaluation
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed
* 4: runtime or device execution error

---

## Limitations

* The script name must match a script in the recipe
* Phase 2 uses `FakeDevice`; real hardware is not implemented yet
* Only one script can be executed per command

---

## Troubleshooting

* If the command reports `Script '<name>' was not found`, verify the script name in the recipe file
* If the command exits with `3`, validate the recipe/spec pair before re-running
* If the selected script returns unexpected data, inspect `session.log` and `result.json`
