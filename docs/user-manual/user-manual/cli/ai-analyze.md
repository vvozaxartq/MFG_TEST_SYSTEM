# Command: ai analyze

## Description

Analyze a completed run artifact using the Phase 3A deterministic rule-based analyzer.

The analyzer reads a saved `result.json`, builds a normalized summary model, and reports rule-based observations.

When available, it can also read `session.events.jsonl` to enrich the normalized summary with variable-resolution failures, exception-style events, warning counts, and first-failure messages.

It does not access runtime engine internals, does not require UI, and does not call any network-based AI service.

---

## Syntax

```bash
ats ai analyze --result-json <file>
```

```bash
ats ai analyze --result-json <file> --events-jsonl <file>
```

```bash
ats ai analyze --result-json <file> --events-jsonl <file> --output-json <file>
```

```bash
ats ai analyze --result-json <file> --events-jsonl <file> --output-json <file> --output-bundle-json <file>
```

```bash
ats ai analyze --result-json <file> --events-jsonl <file> --output-json <file> --output-bundle-json <file> --provider fake
```

---

## Parameters

* `--result-json <file>`: required path to a `result.json` artifact produced by `ats test simulate`, `ats test run`, or `ats script run`
* `--events-jsonl <file>`: optional path to the matching `session.events.jsonl` artifact for richer deterministic analysis
* `--output-json <file>`: optional path to write the full `AiRunAnalysisResult` as a standalone JSON artifact
* `--output-bundle-json <file>`: optional path to write the full `AiAnalysisBundle` contract, including artifact metadata, normalized summary, and explainable analysis
* `--provider fake`: optional provider placeholder switch that routes the generated `AiAnalysisBundle` through the deterministic fake provider adapter and prints provider-style output

---

## Example

```bash
ats ai analyze --result-json .codex-out\demo\result.json
```

```bash
ats ai analyze --result-json artifacts\test-run\result.json
```

```bash
ats ai analyze --result-json artifacts\test-run\result.json --events-jsonl artifacts\test-run\session_123.events.jsonl
```

```bash
ats ai analyze --result-json artifacts\test-run\result.json --events-jsonl artifacts\test-run\session_123.events.jsonl --output-json artifacts\test-run\analysis.json
```

```bash
ats ai analyze --result-json artifacts\test-run\result.json --events-jsonl artifacts\test-run\session_123.events.jsonl --output-json artifacts\test-run\analysis.json --output-bundle-json artifacts\test-run\analysis-bundle.json
```

```bash
ats ai analyze --result-json artifacts\test-run\result.json --events-jsonl artifacts\test-run\session_123.events.jsonl --provider fake
```

---

## Expected Output

* console output only
* analyzer name
* primary category
* primary cause
* optional confidence
* one-line run summary including run status and key counts
* deterministic observations such as pass summary, spec failures, execution errors, failed steps, variable resolution failures, unhandled exceptions, or warning-level event counts
* structured evidence showing the normalized facts that caused the matched rule to fire
* recommended next actions
* optional standalone analysis artifact when `--output-json` is supplied
* optional provider-ready bundle artifact when `--output-bundle-json` is supplied
* optional provider placeholder section when `--provider fake` is supplied

Example console shape:

```text
Command: ai analyze
Target: E:\runs\result.json
Events: E:\runs\session.events.jsonl
Analyzer: RuleBasedRunAnalyzer
Primary Category: Configuration
Primary Cause: Variable resolution failed before the run could complete.
Confidence: 0.98
Summary: Run status=Error, steps=3, failedSpecs=1, errors=1, variableFailures=1, exceptions=0, warnings=0.
[Error] Variable Resolution Failures: Detected 1 variable resolution failures. First failure: Variable 'dut.sn' required by field 'Command' in step 'ReadDut' was not found in DutContext.
Recommended Actions:
- Inspect session.events.jsonl for VariableResolutionFailed entries and confirm the first missing placeholder.
- Verify the required value exists in Step, DUT, or Global inputs before rerunning.
Evidence:
- [Metric] Variable resolution failure count | source=RunArtifactSummary.VariableResolutionFailedCount | value=1
- [State] Variable resolution failure flag | source=RunArtifactSummary.HasVariableResolutionFailures | value=True
- [Message] First failure message | source=RunArtifactSummary.FirstFailureMessage | value=Variable 'dut.sn' required by field 'Command' in step 'ReadDut' was not found in DutContext.
Provider: fake
Provider Bundle Schema: ats.ai-analysis-bundle.v1
Provider Primary Category: Configuration
Provider Primary Cause: Variable resolution failed before the run could complete.
Provider Summary: Fake provider consumed bundle schema=ats.ai-analysis-bundle.v1, runStatus=Error, primaryCategory=Configuration, matchedRules=1, evidence=1.
Provider Highlights:
- Bundle schema ats.ai-analysis-bundle.v1 for session SESSION-001 was consumed by the fake provider.
- Matched rules: VariableResolutionFailureRule.
```

Example exported JSON shape:

```json
{
  "analyzerName": "RuleBasedRunAnalyzer",
  "primaryCategory": "Configuration",
  "primaryCause": "Variable resolution failed before the run could complete.",
  "confidence": 0.98,
  "matchedRules": [
    "VariableResolutionFailureRule"
  ],
  "evidence": [
    {
      "type": "Metric",
      "message": "Variable resolution failure count",
      "source": "RunArtifactSummary.VariableResolutionFailedCount",
      "value": "1"
    }
  ]
}
```

Example exported bundle shape:

```json
{
  "SchemaVersion": "ats.ai-analysis-bundle.v1",
  "GeneratedAtUtc": "2026-04-04T13:53:00Z",
  "AnalyzerName": "RuleBasedRunAnalyzer",
  "ResultJsonPath": "E:\\runs\\result.json",
  "EventsJsonlPath": "E:\\runs\\session.events.jsonl",
  "AnalysisJsonPath": "E:\\runs\\analysis.json",
  "Summary": {
    "SessionId": "SESSION-001",
    "CommandName": "test run",
    "RunStatus": "Error",
    "VariableResolutionFailedCount": 1,
    "HasVariableResolutionFailures": true
  },
  "Analysis": {
    "PrimaryCategory": "Configuration",
    "PrimaryCause": "Variable resolution failed before the run could complete.",
    "MatchedRules": [
      "VariableResolutionFailureRule"
    ],
    "Evidence": [
      {
        "Type": "Metric",
        "Message": "Variable resolution failure count",
        "Source": "RunArtifactSummary.VariableResolutionFailedCount",
        "Value": "1"
      }
    ]
  }
}
```

---

## Exit Codes

* 0: analysis completed successfully
* 2: invalid CLI syntax or missing required parameters
* 4: result artifact or events artifact could not be found, parsed, or was not a supported run artifact

---

## Limitations

* Phase 3A only supports `result.json` artifacts from `test simulate`, `test run`, and `script run`
* Event-aware enrichment only works when a matching `session.events.jsonl` is provided
* Analysis is deterministic and rule-based; it is not a general assistant and does not generate free-form AI reasoning
* Primary cause selection follows a fixed precedence so the most actionable failure class is shown first
* Exported analysis JSON is a report artifact only; it does not change the original run result
* Exported bundle JSON is a normalized contract for downstream consumers; it intentionally omits raw event-stream payload lines
* `--provider fake` is only a placeholder adapter; it does not call a real model or provider
* The analyzer consumes normalized summary data, so it does not inspect live device state or in-memory flow-engine objects
* No network calls or external AI model integrations are used

---

## Troubleshooting

* If the command reports that `result.json` was not found, verify the path printed by the original run command
* If the command reports that `events.jsonl` was not found, verify the matching path printed by the original run command
* If the command reports an unsupported artifact, make sure the file came from `ats test simulate`, `ats test run`, or `ats script run`
* If the output shows a different primary cause than you expected, check the structured events first; higher-priority rule matches can intentionally override lower-priority symptom summaries
* If you need to preserve the full analysis for later review, rerun with `--output-json <file>` and inspect the `matchedRules` and `evidence` arrays
* If downstream tooling needs one stable contract instead of separate files, rerun with `--output-bundle-json <file>` and consume the top-level `Summary` and `Analysis` objects
* If `--provider` is set to anything other than `fake`, the command returns an invalid-arguments error because no real provider integrations exist yet
* If variable resolution failures or exception observations appear, inspect the matching `session.events.jsonl` and `session.log` alongside `result.json`
