# AutoTestSystem.Next - Codex Plan

## Core Principles

* CLI-first
* Core-first
* UI-optional
* AI-optional

---

## Phase 1 (MVP)

### Goal

Create a minimal runnable CLI test system.

### Must Implement

* ScriptBase
* TestContext
* DataCollection
* TestResult
* IDevice
* FakeDevice
* FlowEngine (minimal)
* RecipeLoader (minimal)
* SpecEngine (minimal)

### Target Command

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

### Expected Outputs

* result.json
* result.csv
* session.log

---

## Phase 2

* ats script run
* ats test run
* ats device exec
* recipe validate
* spec validate

---

## Phase 3

* AI interface (placeholder)
* Thin UI for artifact viewing and CLI triggering
* UI must remain optional and must not replace CLI/Core as the source of truth

---

## Required Future Work (MUST DO)

The following items are mandatory for station-grade manufacturing use and must be treated as required work, not optional nice-to-have items.

### Multi-DUT Station Execution

* Support one station process / one station session running multiple DUTs in the same run
* Add explicit per-DUT runtime/context instead of assuming one `DutContext` per session
* Keep CLI-first and Core-first; do not introduce UI dependency for this work

### Per-DUT Variable Mapping

* Support shared recipe/script templates with per-DUT variable injection
* Support channel/resource-style variable mapping such as `poweron=1`, `poweron=2`
* Support loading per-DUT variable values from a table/config source instead of hardcoding them in scripts
* Define clear variable precedence for Step > DUT > Station/Session > Global defaults

### Shared Resource Management

* Add explicit shared resource management for instruments, IO cards, switch cards, PSU channels, and similar station resources
* Treat lock handling as mandatory when resources are shared
* Support at least:
  * device-level lock
  * channel-level lock
  * grouped step/resource lock
* Generate lock keys from resolved runtime resource values, not from unresolved template names

### Logging And Traceability Requirements

* Make multi-DUT traceability mandatory in runtime artifacts and logs
* Every important runtime event must be able to identify:
  * `sessionId`
  * `dutId`
  * `sn`
  * `slot`
  * `channel`
  * `stepName`
  * `resourceName` / `resourceKey`
* Add explicit structured log events for lock lifecycle, including:
  * `ResourceLockRequested`
  * `ResourceLockAcquired`
  * `ResourceLockReleased`
  * `ResourceLockTimeout`
  * `ResourceLockDenied`
* Ensure human-readable `session.log` can distinguish the same script running concurrently for different DUTs

### Data Isolation Requirements

* Do not allow shared-session measurement collisions across DUTs
* Replace `fullKey`-only isolation with per-DUT isolation such as `dutId + fullKey`
* Ensure results, measurements, spec evaluations, and DataCollection all remain traceable per DUT

### Compatibility Constraints

* Keep existing single-DUT CLI flows working
* Do not break existing Phase 1 / Phase 2 command behavior while adding station-grade multi-DUT support
* Use FakeDevice-first and simulation-first for early implementation slices

---

## Rules

* Always ensure solution builds
* Always ensure CLI works
* Do NOT implement UI early
* Use FakeDevice first
* Keep implementation minimal first

---

## Documentation Requirement

For every major feature:

* update CHANGELOG.md
* update docs/user-manual/*.md
* ensure CLI behavior matches documentation
