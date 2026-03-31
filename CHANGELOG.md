# CHANGELOG

## [INIT]

### Added

* Repository initialized
* README.md created
* CODEX_PLAN.md created
* AGENTS.md created
* CHANGELOG.md created

### Notes

* Initial empty project setup

## [2026-03-31 23:16]

### Added

* Added `ATS.Core`, `ATS.Application`, `ATS.Cli`, and `ATS.Tests` for the Phase 1 CLI-first MVP
* Added `ScriptBase`, `TestContext`, `DataCollection`, `TestResult`, `IDevice`, `FakeDevice`, `FlowEngine`, `RecipeLoader`, and `SpecEngine`
* Added sample recipe `samples/recipes/demo.recipe.json`
* Added CLI artifact generation for `result.json`, `result.csv`, and `session.log`

### Modified

* Updated CLI documentation to describe the actual Phase 1 command surface and output behavior
* Updated getting started documentation with build, run, exit code, limitation, and troubleshooting details

### Fixed

* Fixed the repository state from documentation-only scaffolding to a runnable `ats test simulate` implementation

### Notes

* Phase 1 was implemented as a minimal CLI-first simulation flow with `FakeDevice` and no UI

## [2026-03-31 23:50]

### Added

* Added Phase 2 CLI commands: `ats test run`, `ats script run`, `ats device exec`, `ats recipe validate`, and `ats spec validate`
* Added `DeviceCommandRequest` / `DeviceCommandResponse`, session artifact writing, recipe/spec validation services, and shared test runner infrastructure
* Added `samples/recipes/phase2.recipe.json` and `samples/specs/phase2.spec.json` for Phase 2 execution and validation examples

### Modified

* Strengthened `FlowEngine`, `TestRunner`, and `SpecEngine` to preserve artifacts on failures and support `Equal`, `Range`, `GreaterThan`, `LessThan`, `Regex`, `Contain`, `NotEqual`, and `Bypass`
* Expanded CLI parsing, help text, and exit code handling while preserving Phase 1 `ats test simulate`
* Updated user manual pages to document Phase 2 commands, parameters, examples, outputs, exit codes, limitations, and troubleshooting notes

### Fixed

* Fixed failed or invalid runs so `result.json`, `result.csv`, and `session.log` remain available for debugging

### Notes

* Phase 2 remains CLI-first, Core-first, FakeDevice-only, and does not add UI
