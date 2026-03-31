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
* UI placeholder (no real implementation)

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
