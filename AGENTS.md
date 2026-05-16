# AGENTS.md

## Project Overview

CLI-first manufacturing test system.

---

## 🚨 Critical Rules (MUST FOLLOW)

1. DO NOT prioritize UI
2. ALWAYS follow CODEX_PLAN.md
3. Implement phase by phase
4. Always ensure project builds
5. Always ensure CLI command works

---

## 🔥 CHANGE TRACKING (REQUIRED)

Every change MUST update CHANGELOG.md

### Required format

## [YYYY-MM-DD HH:mm]

### Added

* ...

### Modified

* ...

### Fixed

* ...

### Notes

* reason / impact

### Rules

* MUST update CHANGELOG.md in same change
* MUST describe what changed
* MUST describe why
* If missing CHANGELOG update → task is NOT complete

---

## 📘 DOCUMENTATION RULES (REQUIRED)

For every major feature or user-visible change:

### MUST update:

1. CHANGELOG.md
2. docs/user-manual/*.md

### Documentation must include:

* feature purpose
* CLI syntax
* parameters
* examples
* expected output
* exit codes (if relevant)
* limitations
* troubleshooting notes

### Rules

* Markdown is the source of truth
* HTML should be generated from Markdown (do not manually maintain HTML)
* If documentation is missing → task is NOT complete

---

## 🧠 CODING RULES

* Keep code minimal and working first
* Do not over-engineer
* Do not introduce complex patterns early
* Add unit tests for core logic
* Do not refactor unrelated code

---

## 🏗️ ARCHITECTURE RULES

* Core must not depend on UI
* CLI must run independently
* Device layer must be abstracted
* Use FakeDevice first before real hardware

---

## 🔢 PRIORITY ORDER

1. ATS.Core
2. ATS.Application
3. ATS.Cli
4. ATS.Infrastructure
5. ATS.Hardware
6. UI (last)

---

## 📦 OUTPUT RULES

* Always produce buildable code
* Always ensure CLI works
* Always provide minimal working version
* Do not break existing functionality

---

## FAST REVIEW WORKFLOW (REQUIRED)

Before doing broad repo scans, read files in this order:

1. `AGENTS.md`
2. `CODEX_PLAN.md`
3. `CODEX_CONTEXT.md`
4. `CODEX_CONTEXT_SNAPSHOT.md`
5. `CHANGELOG.md`

Rules:

* Use `CODEX_CONTEXT.md` for stable project summary
* Use `CODEX_CONTEXT_SNAPSHOT.md` for current filtered worktree summary
* Regenerate `CODEX_CONTEXT_SNAPSHOT.md` after meaningful repo changes
* Prefer filtered summaries over reading large generated artifacts unless the task is artifact-specific

## CONTEXT UPDATE DISCIPLINE (REQUIRED)

After every completed change:

1. Update `CHANGELOG.md`
2. Regenerate `CODEX_CONTEXT_SNAPSHOT.md` with `powershell -ExecutionPolicy Bypass -File tools/update-codex-context.ps1`
3. Update `CODEX_CONTEXT.md` only when the stable project summary, architecture state, phase status, or must-do future work has materially changed

Rules:

* `CHANGELOG.md` is the canonical per-change history
* `CODEX_CONTEXT_SNAPSHOT.md` is the canonical current-state fast review file
* `CODEX_CONTEXT.md` is not a per-edit log; keep it concise and stable
