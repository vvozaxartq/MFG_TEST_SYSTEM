# Command: spec validate

## Description

Validate spec definitions and supported operators before test execution.

---

## Syntax

```bash
ats spec validate --spec <file> [--output <directory>]
```

---

## Parameters

* `--spec <file>`: path to spec JSON file
* `--output <directory>`: optional output directory, defaults to current working directory

---

## Example

```bash
ats spec validate --spec samples/specs/phase2.spec.json
```

---

## Expected Output

* validation summary on console
* result.json
* result.csv
* session.log

---

## Exit Codes

* 0: spec validation passed
* 2: invalid CLI syntax or missing required parameters
* 3: spec validation failed

---

## Supported Operators

* `Equal`
* `Range`
* `GreaterThan`
* `LessThan`
* `Regex`
* `Contain`
* `NotEqual`
* `Bypass`

---

## Limitations

* Numeric operators require numeric values in the spec and the actual response
* Only JSON spec files are supported
* Validation checks definitions only; it does not run any device commands

---

## Troubleshooting

* If validation reports an unsupported operator, verify the operator spelling
* If validation reports missing numeric values, check `expected`, `minimum`, or `maximum` fields
* Use `result.json` or `result.csv` to review all reported validation issues
