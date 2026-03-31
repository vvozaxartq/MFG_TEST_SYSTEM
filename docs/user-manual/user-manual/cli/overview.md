# CLI Overview

## Command Structure

```bash
ats <category> <action> [options]
```

---

## Phase 2 Commands

* `test simulate`
* `test run`
* `script run`
* `device exec`
* `recipe validate`
* `spec validate`

---

## Example

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

```bash
ats test run --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

---

## Supported Options

* `--recipe <file>`: recipe JSON path for `test`, `script`, and `recipe` commands
* `--spec <file>`: optional external spec JSON path for `test`, `script`, and `recipe` commands
* `--script <name>`: required when using `ats script run`
* `--command <text>`: required when using `ats device exec`
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Output Types

* console output
* JSON
* CSV
* log files

---

## Notes

* Phase 2 is still CLI-first and does not include UI
* All execution commands use `FakeDevice`
* `test simulate` remains available for Phase 1 compatibility

---

## Exit Codes

* `0`: success
* `1`: test or script completed with failed spec result
* `2`: invalid CLI arguments
* `3`: recipe or spec validation failed
* `4`: runtime or device execution error
