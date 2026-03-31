# User Manual

## Overview

AutoTestSystem.Next is a CLI-first manufacturing test system.

It allows running test flows without requiring a graphical interface.

---

## Main Capabilities

* Run full test sessions via CLI
* Run a single script for debugging
* Execute raw FakeDevice commands for troubleshooting
* Validate recipe and spec files before execution
* Simulate test without hardware
* Export results (JSON / CSV / logs)

---

## Entry Point

All operations are executed via CLI:

```bash
ats <command> [options]
```

---

## Included In Phase 2

* `test simulate`
* `test run`
* `script run`
* `device exec`
* `recipe validate`
* `spec validate`
