# Command: recipe validate

## Description

Validate recipe structure and recipe-to-spec mapping before test execution.

---

## Syntax

```bash
ats recipe validate --recipe <file> [--spec <file>] [--output <directory>]
```

---

## Parameters

* `--recipe <file>`: path to recipe JSON file
* `--spec <file>`: optional external spec JSON file; if omitted, inline recipe specs are used
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Example

```bash
ats recipe validate --recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json
```

---

## Expected Output

* validation summary on console
* result.json
* result.csv
* session.log

---

## Exit Codes

* 0: recipe validation passed
* 2: invalid CLI syntax or missing required parameters
* 3: recipe or spec validation failed

---

## Limitations

* Validation checks JSON structure, required fields, duplicate names/keys, and spec references
* Validation does not run device commands
* Only JSON recipe and spec files are supported

---

## Troubleshooting

* If validation reports missing specs, either add inline specs or pass `--spec`
* If validation reports duplicate script names, rename scripts to unique values
* Inspect `result.csv` for a compact error list when multiple issues are reported
