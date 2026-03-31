# AutoTestSystem.Next

Next generation manufacturing test system.

## Core Concepts

* CLI-first
* Core-first
* UI-optional
* AI-optional

## Goals

* Run test flows without UI
* Support command line execution
* Modular architecture (Core / Device / CLI / UI)
* Extensible for automation and AI assistance

## Getting Started

```bash
dotnet build
dotnet run --project src/ATS.Cli
```

## Example

```bash
ats test simulate --recipe samples/recipes/demo.recipe.json
```

## Structure

* `src/` - source code
* `tests/` - unit tests
* `samples/` - sample data
* `docs/` - user manual and documentation

## Notes

UI is optional and will be implemented later.
Core and CLI are primary focus.
