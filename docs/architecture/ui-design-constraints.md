# UI Design Constraints

This document defines constraints that must be respected when implementing UI.

---

## Core Principle

UI is NOT the source of truth.

* Core logic must live in ATS.Core / ATS.Application
* UI must only consume data and trigger commands
* UI must NOT contain business logic

---

## Data Model Constraints

UI must be built on top of these stable models:

### Measurement System

* MeasurementSet
* MeasurementItem
* fullKey (prefix.key)

UI must always display:

* fullKey
* value
* unit

---

### Spec System

* SpecRule
* SpecEvaluationResult

UI must:

* show each rule result
* show pass/fail per key
* show failure reason

---

### Prefix / Namespace

UI must treat fullKey as unique identifier.

Example:

* battery.voltage
* usb.voltage

UI must NOT rely on raw key only.

---

## Multi DUT Design (Future)

System will support multiple DUTs.

UI must be designed to support:

* multiple DUT contexts
* same flow, different variables
* same key but different prefix (dut1.*, dut2.*)

UI must NOT assume single DUT.

---

## Variable System

System supports variable resolution:

* ${varName}
* Scope: Step > DUT > Global

UI must support:

* displaying resolved values
* showing variable sources (optional)

---

## Device / Resource Mapping

Devices may be shared across DUTs.

Example:

* MainDAQ used by DUT1 and DUT2
* DUT1 uses CH1
* DUT2 uses CH2

UI must:

* display logical resource (e.g. power.on)
* not depend on raw hardware channel (CH1 / CH2)

---

## Run Session

Each execution is a session.

UI must support:

* session list
* session detail
* log viewer
* result viewer

Session includes:

* SN
* start/end time
* result
* artifact paths

---

## Logging

Each run generates independent log.

UI must:

* show structured log
* support filtering by step
* support measurement & spec sections

---

## CLI-first Rule

All UI functionality must be achievable via CLI.

UI is a layer on top of CLI, not parallel logic.

---

## Future UI Scope

UI should be designed in phases:

Phase 1:

* run session viewer
* log viewer
* result viewer
* thin CLI runner shell

Phase 2:

* manual trigger
* parameter input (SN, etc.)

Phase 3:

* recipe editor
* device mapping editor

---

## Current Implementation Note

The current repository now includes a thin UI implementation for Phase 1 scope:

* artifact/session viewer
* structured log viewer
* result viewer
* thin CLI runner

This implementation must continue to obey all constraints above and must not move business logic out of CLI/Core layers.
