# Command: ai regress

## Description

Check whether a candidate `analysis-bundle.json` regressed relative to a baseline `analysis-bundle.json`.

The command is deterministic, artifact-based, and fully offline. It does not rerun analysis, inspect live engine state, or depend on any provider integration.

It is intended for bundle-to-bundle baseline verification after `ats ai analyze --output-bundle-json <file>` has already been run for both artifacts.

---

## Syntax

```bash
ats ai regress --baseline-bundle <file> --candidate-bundle <file> --output-json <file> --output-html <file>
```

---

## Parameters

* `--baseline-bundle <file>`: required path to the baseline `analysis-bundle.json` artifact
* `--candidate-bundle <file>`: required path to the candidate `analysis-bundle.json` artifact
* `--output-json <file>`: required path for the machine-readable regression result JSON
* `--output-html <file>`: required path for the offline HTML regression report

---

## Regression Conditions

The deterministic regression checker currently flags regressions when:

* primary category becomes worse
* failed spec count increases
* exception count increases
* variable resolution failures appear or increase
* new failed step names appear
* more severe matched rules appear

---

## Example

```bash
ats ai regress --baseline-bundle artifacts\baseline\analysis-bundle.json --candidate-bundle artifacts\candidate\analysis-bundle.json --output-json artifacts\regression\regression.json --output-html artifacts\regression\regression.html
```

---

## Expected Output

* a machine-readable regression JSON artifact
* an offline HTML regression report
* regression status
* summary text
* explicit regression findings
* baseline vs candidate compared facts

Example console shape:

```text
Command: ai regress
Baseline Bundle: E:\runs\baseline\analysis-bundle.json
Candidate Bundle: E:\runs\candidate\analysis-bundle.json
Output JSON: E:\runs\regression\regression.json
Output HTML: E:\runs\regression\regression.html
Regression Status: RegressionDetected
Finding Count: 3
Summary: Detected 3 regression findings relative to the baseline bundle.
[PrimaryCategoryWorsened] Primary category worsened from Success to Execution.
```

---

## Exit Codes

* 0: regression artifacts were written successfully
* 2: invalid CLI syntax or missing required parameters
* 4: a bundle artifact could not be found or parsed

---

## Limitations

* `ai regress` expects `analysis-bundle.json` artifacts as both inputs
* The current regression rules are deterministic only; no provider or ML-based reasoning is used
* The command writes findings to JSON/HTML artifacts and console output, but it does not currently return a non-zero exit code when regressions are detected
* The report does not embed raw `session.events.jsonl` streams or execution-engine state
* No network calls, web server, or UI framework app are introduced

---

## Troubleshooting

* If the command reports that a bundle file was not found, verify the paths from the earlier `ats ai analyze --output-bundle-json` runs
* If the command reports that a bundle could not be parsed, confirm the file is a valid `analysis-bundle.json` artifact instead of a raw `analysis.json`
* If the regression result seems incomplete, confirm both baseline and candidate bundles were regenerated from the intended runs before checking them
* If the HTML or JSON file is missing after a successful run, verify the output directories were writable
* If you need newer regression results, regenerate both bundles first, then rerun `ats ai regress`
