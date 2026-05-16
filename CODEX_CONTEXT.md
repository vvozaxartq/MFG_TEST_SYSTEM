# CODEX_CONTEXT

## Purpose

This file is the fastest human-maintained summary for future Codex sessions.
Read this before scanning the whole repository.

## Read Order

1. `AGENTS.md`
2. `CODEX_PLAN.md`
3. `CODEX_CONTEXT.md`
4. `CODEX_CONTEXT_SNAPSHOT.md`
5. `CHANGELOG.md`

## Current Product State

* The system is CLI-first and Core-first.
* Phase 1 and Phase 2 are implemented and must stay working.
* Phase 3 currently includes AI artifact analysis plus a thin optional UI that only reads artifacts and invokes the CLI.
* The CLI remains the source of truth. UI must stay thin and optional.
* `FakeDevice` is still the default execution path. Real hardware support is not the current priority.

## Project Map

* `src/ATS.Core`
  Core models, contracts, session/result/spec/measurement/log structures.
* `src/ATS.Application`
  Execution flow, loaders, validators, artifact writers, structured logging, AI artifact analysis.
* `src/ATS.Cli`
  Command parsing and CLI entrypoint.
* `src/ATS.Ui`
  Thin Windows UI for artifact viewing and CLI triggering only.
* `tests/ATS.Tests`
  Buildable executable test harness for core and application behavior.

## Current Must-Do Future Work

These are already recorded in `CODEX_PLAN.md` and should be treated as mandatory:

* Multi-DUT station execution in one station session
* Per-DUT variable mapping for shared recipe/script templates
* Shared resource management for instruments, IO cards, channels, and grouped locks
* Lock-aware structured logging and human-readable traceability
* Per-DUT data isolation so measurements and spec results do not collide

## Validation Commands

```powershell
dotnet build AutoTestSystem.Next.sln -m:1
dotnet run --project src/ATS.Cli -- test simulate --recipe samples/recipes/demo.recipe.json
dotnet run --project tests/ATS.Tests
```

## Token-Saving Workflow

* Start with this file and `CODEX_CONTEXT_SNAPSHOT.md` before opening many source files.
* Use the snapshot to see current branch, current HEAD, and filtered working tree changes.
* Avoid scanning generated artifacts unless the task is explicitly about artifacts or debugging output.
* Prefer filtered `git status --short` over broad recursive file reads.
* Update the snapshot after meaningful code or document changes.

## Update Discipline

When a task changes repo state in a meaningful way:

1. Update `CHANGELOG.md`
2. Regenerate `CODEX_CONTEXT_SNAPSHOT.md`
3. Update this file only if the stable project story changed

### Practical Rule

* Every completed change should appear in `CHANGELOG.md`
* Every completed change should refresh `CODEX_CONTEXT_SNAPSHOT.md`
* Only stable context changes should update `CODEX_CONTEXT.md`

## Snapshot Command

```powershell
powershell -ExecutionPolicy Bypass -File tools/update-codex-context.ps1
```
