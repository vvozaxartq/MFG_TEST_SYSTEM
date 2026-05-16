# Command: ai compare

## Description

Compare two saved `analysis-bundle.json` artifacts and render an offline HTML comparison report.

The command is bundle-first and artifact-based. It does not rerun analysis, inspect live runtime state, or require a web server.

It is intended for regression review, side-by-side troubleshooting, and offline comparison of normalized summary facts plus explainable analysis output.

---

## Syntax

```bash
ats ai compare --left-bundle <file> --right-bundle <file> --output-html <file>
```

---

## Parameters

* `--left-bundle <file>`: required path to the left-side `analysis-bundle.json` artifact
* `--right-bundle <file>`: required path to the right-side `analysis-bundle.json` artifact
* `--output-html <file>`: required path for the generated offline HTML comparison report

---

## Example

```bash
ats ai analyze --result-json artifacts\run-a\result.json --events-jsonl artifacts\run-a\session_a.events.jsonl --output-bundle-json artifacts\run-a\analysis-bundle.json
```

```bash
ats ai analyze --result-json artifacts\run-b\result.json --events-jsonl artifacts\run-b\session_b.events.jsonl --output-bundle-json artifacts\run-b\analysis-bundle.json
```

```bash
ats ai compare --left-bundle artifacts\run-a\analysis-bundle.json --right-bundle artifacts\run-b\analysis-bundle.json --output-html artifacts\compare\analysis-compare.html
```

---

## Expected Output

* a standalone `.html` comparison report
* left vs right summary cards
* primary category / cause / confidence change table
* changed normalized summary counts
* changed failed step names
* changed matched rules
* changed evidence items
* changed recommended actions

Example console shape:

```text
Command: ai compare
Left Bundle: E:\runs\run-a\analysis-bundle.json
Right Bundle: E:\runs\run-b\analysis-bundle.json
Output: E:\runs\compare\analysis-compare.html
Left Primary Category: Configuration
Right Primary Category: Runtime
Differences Detected: Yes
Changed Summary Counts: 4
Changed Matched Rules: 3
```

---

## Exit Codes

* 0: comparison HTML report rendered successfully
* 2: invalid CLI syntax or missing required parameters
* 4: a bundle artifact could not be found or parsed

---

## Limitations

* `ai compare` expects `analysis-bundle.json` artifacts as both inputs
* The comparison is based on normalized summary facts and explainable analysis content already stored in each bundle
* The report is offline HTML only; it does not start a web server or create a framework-based UI app
* The report does not embed raw `session.events.jsonl` lines or execution engine state
* No provider calls, network calls, or external JavaScript/CSS dependencies are used

---

## Troubleshooting

* If the command reports that a bundle file was not found, verify the paths from the earlier `ats ai analyze --output-bundle-json` runs
* If the command reports that a bundle could not be parsed, confirm the file is a valid `analysis-bundle.json` artifact instead of a raw `analysis.json`
* If the comparison report is missing expected changes, verify both bundles were regenerated from the intended runs before comparing them
* If the HTML file is missing after a successful run, check that the target directory was writable
* If you need a newer comparison, regenerate each bundle first with `ats ai analyze`, then rerun `ats ai compare`
