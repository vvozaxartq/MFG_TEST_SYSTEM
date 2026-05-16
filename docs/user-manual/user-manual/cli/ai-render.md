# Command: ai render

## Description

Render a saved `analysis-bundle.json` artifact into an interactive offline HTML analysis viewer.

The renderer consumes the provider-ready `AiAnalysisBundle` contract and produces a readable offline report with lightweight client-side interactivity, without starting a web server or depending on any UI framework app.

It is intended for artifact review, sharing, and troubleshooting after `ats ai analyze --output-bundle-json <file>` has already been run.

---

## Syntax

```bash
ats ai render --bundle-json <file> --output-html <file>
```

---

## Parameters

* `--bundle-json <file>`: required path to an `analysis-bundle.json` artifact produced from `ats ai analyze`
* `--output-html <file>`: required path for the rendered static HTML viewer file

---

## Example

```bash
ats ai analyze --result-json artifacts\test-run\result.json --events-jsonl artifacts\test-run\session_123.events.jsonl --output-bundle-json artifacts\test-run\analysis-bundle.json
```

```bash
ats ai render --bundle-json artifacts\test-run\analysis-bundle.json --output-html artifacts\test-run\analysis-viewer.html
```

---

## Expected Output

* a standalone `.html` file
* bundle header / artifact metadata
* primary category
* primary cause
* confidence
* collapsible sections
* search box for messages, rules, evidence, and step names
* severity/category filters where applicable
* summary
* observations
* recommended actions
* evidence
* normalized summary facts

Example console shape:

```text
Command: ai render
Bundle: E:\runs\analysis-bundle.json
Output: E:\runs\analysis-viewer.html
Analyzer: RuleBasedRunAnalyzer
Primary Category: Configuration
Primary Cause: Variable resolution failed before the run could complete.
Schema Version: ats.ai-analysis-bundle.v1
```

---

## Exit Codes

* 0: HTML viewer rendered successfully
* 2: invalid CLI syntax or missing required parameters
* 4: bundle artifact could not be found or parsed

---

## Limitations

* `ai render` expects `analysis-bundle.json` as its primary input contract
* The viewer is self-contained HTML with inline JavaScript only; it does not start a web server
* No live refresh, UI framework app, or execution-engine integration is included
* The command does not regenerate analysis; it only renders an existing bundle
* If JavaScript is disabled, the page remains readable, but search and filters will not be active
* No network calls or external UI framework dependencies are used

---

## Troubleshooting

* If the command reports that `bundle-json` was not found, verify the path from the earlier `ats ai analyze --output-bundle-json` run
* If the command reports that the bundle could not be parsed, make sure the file is a valid `analysis-bundle.json` artifact and not a raw `analysis.json`
* If the HTML file is missing after a successful run, verify the parent directory was writable
* If search or filters do not respond, make sure the browser allows inline JavaScript for local HTML files
* If you need a newer viewer report, rerun `ats ai analyze` first to regenerate the bundle, then run `ats ai render`
