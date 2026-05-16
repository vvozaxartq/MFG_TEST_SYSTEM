# User Manual

## Overview

AutoTestSystem.Next is a CLI-first manufacturing test system.

It allows running test flows without requiring a graphical interface.

It now also includes a thin Windows UI layer for artifact viewing and CLI triggering, while keeping CLI/Core as the source of truth.

---

## Main Capabilities

* Run full test sessions via CLI
* Review completed sessions in a thin Windows UI without re-implementing core logic
* Run a single script for debugging
* Execute raw FakeDevice commands for troubleshooting
* Validate recipe and spec files before execution
* Simulate test without hardware
* Parse one script response into multiple normalized measurements with step-owned `prefix` / `fullKey` namespacing
* Evaluate multiple spec rules independently against the same step result and keep per-rule pass/fail output in artifacts
* Apply deterministic step retry, timeout, and continue-on-failure policies during recipe execution
* Run nested sequence, simple branch flows, and deterministic `repeatUntil` loops from recipe-defined flow trees
* Control sequence container termination with explicit deterministic outcome policies
* Control repeat loop termination with explicit deterministic outcome policies
* Export explicit flow result trees in `result.json` for containers, branch choices, repeat iterations, and node timing
* Analyze completed run artifacts with a deterministic rule-based analyzer
* Enrich run analysis with optional structured event evidence from `session.events.jsonl`
* Export standalone explainable analysis JSON artifacts for later review
* Export provider-ready analysis bundles with normalized summary, metadata, and explainable findings
* Exercise a deterministic provider adapter placeholder from exported bundle data
* Render interactive offline HTML analysis viewers from saved bundle artifacts
* Compare two saved analysis bundles with an offline HTML comparison report
* Check whether a candidate analysis bundle regressed relative to a baseline bundle
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

## Added In Phase 3A

* `ai analyze`

## Added In Phase 3B

* `ai render`
* `ai compare`

## Added In Phase 3C

* `ai regress`

## Thin UI

* `ATS.Ui`: artifact viewer + CLI runner
