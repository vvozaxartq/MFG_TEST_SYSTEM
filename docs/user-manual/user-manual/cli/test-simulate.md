# Command: test simulate

## Description

Run test flow in simulation mode without real hardware.

---

## Syntax

```bash
ats test simulate --recipe <file>
```

---

## Parameters

* recipe: path to recipe JSON file

---

## Example

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

---

## Output

* result.json
* result.csv
* session.log

---

## Notes

* Uses FakeDevice
* Does not require hardware
