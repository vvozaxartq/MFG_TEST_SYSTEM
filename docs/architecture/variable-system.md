# Variable System Contract

## Purpose

This document defines the execution-time variable system used by CLI runs and future UI integrations.

UI must consume resolved data from Core/Application outputs.

UI must not implement its own variable precedence rules.

---

## Syntax

Execution-time variables use the `${varName}` syntax.

Examples:

* `${SN}`
* `${VoltagePath}`
* `READ_${SN}_${CommandSuffix}`
* `${dut.sn}`
* `${dut.isSimulated}`

This syntax is separate from artifact naming templates such as `%SN%` or `{SN}`.

Artifact naming templates are for output path generation.

`${varName}` is only for execution-time recipe and step string resolution.

---

## Scope Order

Variable resolution order is fixed:

1. Step
2. DUT
3. Global

Resolution must use exact variable names.

No fuzzy fallback is allowed.

If a variable is missing, resolution must fail.

If a placeholder starts with `dut.`, it resolves only from `DutContext`.

---

## Models

Core models:

* `VariableScope`
* `ResolvedVariable`
* `VariableContext`
* `DutContext`

Application service:

* `VariableResolver`

`ResolvedVariable` must include:

* `RequestedName`
* `ResolvedName`
* `Value`
* `Scope`
* `Source`

`VariableContext` must include:

* `GlobalVariables`
* `DutContext`
* `StepVariables`

The current implementation also tracks source dictionaries so tools can explain where a resolved value came from.

`DutContext` provides canonical runtime fields:

* `Id`
* `Index`
* `SerialNumber`
* `Station`
* `Slot`
* `IsSimulated`

---

## Current Resolution Targets

Phase 2 resolves variables in these execution-time fields:

* step `Command`
* step `SimulatedResponse`
* measurement `SourcePath`
* measurement `Unit`
* measurement `Description`
* legacy single-measurement `Unit`

Identity fields are not variable-expanded in Phase 2:

* `Prefix`
* measurement `Key`
* `MeasurementKey`
* `SpecKey`
* `SpecRule.TargetKey`

This keeps `fullKey` and spec binding deterministic.

---

## Error Rules

If a variable placeholder is malformed, validation should fail when possible.

Examples:

* `READ_${SN`
* `${ }`
* `${bad name}`

If a variable is referenced at runtime but not found in `Step > DUT > Global`, execution must fail.

The system must not:

* return silent null
* leave the placeholder unchanged
* guess another variable name

Nested variable expansion is not supported in Phase 2.

If a resolved value still contains `${...}`, execution fails instead of recursively expanding it.

---

## Structured Logging

Structured log artifact:

* `session.events.jsonl`

Schema version:

* `ats.structured-log.v1`

Relevant event types:

* `VariableResolved`
* `VariableResolutionFailed`

`sequence` is session-global and strictly increasing.

Variable resolution events should include at least:

* `fieldName`
* `requestedName`
* `resolvedName`
* `scope`
* `source`
* `value`

Variable resolution failure events should include at least:

* `fieldName`
* `requestedName`
* `searchedScopes`

These events are the canonical machine-readable source for future UI, tool, and AI explanation flows.

---

## Single DUT Compatibility

Single-DUT CLI flows now populate `DutContext` from existing run input.

Current mapping:

* `--sn` -> `dut.sn`
* `--station` -> `dut.station`
* `--vars DutId=...`, `DutIndex=...`, `Slot=...` -> remaining DUT fields
* `dut.id` falls back to the active serial number when `DutId` is omitted
* `dut.isSimulated` reflects command mode

`recipe validate` keeps supported `dut.*` placeholders unresolved and marks them as runtime-provided warnings instead of execution-time errors.
