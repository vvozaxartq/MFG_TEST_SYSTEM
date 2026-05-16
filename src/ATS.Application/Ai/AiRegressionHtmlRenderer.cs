using System.Net;
using System.Text;
using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiRegressionHtmlRenderer
{
    public string Render(
        AiRegressionCheckResult result,
        AiAnalysisBundle baselineBundle,
        AiAnalysisBundle candidateBundle)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(baselineBundle);
        ArgumentNullException.ThrowIfNull(candidateBundle);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine("  <title>ATS Regression Check Report</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    :root { --bg: #f4efe7; --card: #fffdf9; --ink: #201c17; --muted: #70675f; --line: #ddd3c4; --good: #0e6a5c; --good-soft: #d8f0ea; --warn: #9b3e19; --warn-soft: #ffe1d5; --base: #145b8f; --base-soft: #e0efff; --cand: #7f3a0e; --cand-soft: #ffe8d7; --shadow: rgba(45, 35, 20, 0.08); }");
        html.AppendLine("    * { box-sizing: border-box; }");
        html.AppendLine("    body { margin: 0; font-family: \"Segoe UI\", \"Noto Sans\", sans-serif; color: var(--ink); background: linear-gradient(180deg, #ece2d2 0%, var(--bg) 100%); }");
        html.AppendLine("    main { max-width: 1160px; margin: 0 auto; padding: 28px 20px 40px; }");
        html.AppendLine("    .hero, .card, details { background: var(--card); border: 1px solid var(--line); border-radius: 18px; box-shadow: 0 12px 32px var(--shadow); }");
        html.AppendLine("    .hero { padding: 24px; }");
        html.AppendLine("    .eyebrow { margin: 0 0 10px; text-transform: uppercase; letter-spacing: 0.14em; font-size: 12px; color: var(--muted); }");
        html.AppendLine("    h1, h2, h3, p { margin-top: 0; }");
        html.AppendLine("    h1 { margin-bottom: 12px; font-size: 34px; }");
        html.AppendLine("    h2 { margin-bottom: 10px; font-size: 22px; }");
        html.AppendLine("    .summary { font-size: 18px; max-width: 74ch; }");
        html.AppendLine("    .grid, .bundle-grid { display: grid; gap: 14px; margin-top: 18px; }");
        html.AppendLine("    .grid { grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); }");
        html.AppendLine("    .bundle-grid { grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); }");
        html.AppendLine("    .card { padding: 16px; }");
        html.AppendLine("    .bundle-card { border-top: 6px solid var(--base); }");
        html.AppendLine("    .bundle-card.candidate { border-top-color: var(--cand); }");
        html.AppendLine("    .label { display: block; margin-bottom: 8px; color: var(--muted); font-size: 12px; text-transform: uppercase; letter-spacing: 0.08em; }");
        html.AppendLine("    .value { font-size: 24px; font-weight: 700; }");
        html.AppendLine("    .value.small { font-size: 18px; }");
        html.AppendLine("    .chip { display: inline-block; padding: 6px 10px; border-radius: 999px; font-size: 12px; font-weight: 700; }");
        html.AppendLine("    .chip.good { background: var(--good-soft); color: var(--good); }");
        html.AppendLine("    .chip.warn { background: var(--warn-soft); color: var(--warn); }");
        html.AppendLine("    .chip.base { background: var(--base-soft); color: var(--base); }");
        html.AppendLine("    .chip.candidate { background: var(--cand-soft); color: var(--cand); }");
        html.AppendLine("    .stack { display: grid; gap: 14px; margin-top: 22px; }");
        html.AppendLine("    details > summary { cursor: pointer; padding: 16px; display: flex; justify-content: space-between; align-items: center; gap: 12px; font-weight: 700; }");
        html.AppendLine("    details[open] > summary { border-bottom: 1px solid var(--line); }");
        html.AppendLine("    .summary-meta { color: var(--muted); font-size: 13px; font-weight: 600; }");
        html.AppendLine("    .section-body { padding: 16px; }");
        html.AppendLine("    table { width: 100%; border-collapse: collapse; }");
        html.AppendLine("    th, td { padding: 10px 12px; border-bottom: 1px solid var(--line); text-align: left; vertical-align: top; font-size: 14px; }");
        html.AppendLine("    th { color: var(--muted); font-weight: 600; }");
        html.AppendLine("    .list { margin: 0; padding-left: 20px; }");
        html.AppendLine("    .list li { margin-bottom: 8px; }");
        html.AppendLine("    .finding { border-left: 4px solid var(--warn); background: var(--warn-soft); border-radius: 12px; padding: 12px 14px; margin-bottom: 10px; }");
        html.AppendLine("    .finding h3 { margin-bottom: 6px; }");
        html.AppendLine("    .meta { color: var(--muted); font-size: 13px; margin-bottom: 6px; }");
        html.AppendLine("    .empty { color: var(--muted); font-style: italic; }");
        html.AppendLine("    code { font-family: Consolas, \"Courier New\", monospace; font-size: 13px; }");
        html.AppendLine("    .footer { margin-top: 18px; color: var(--muted); font-size: 14px; }");
        html.AppendLine("    @media (max-width: 720px) { h1 { font-size: 28px; } .value { font-size: 20px; } main { padding: 18px 12px 28px; } th, td { display: block; width: 100%; } }");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<main>");
        html.AppendLine("  <section class=\"hero\">");
        html.AppendLine("    <p class=\"eyebrow\">ATS Offline Baseline / Candidate Regression Check</p>");
        html.AppendLine("    <h1>Regression Check Report</h1>");
        html.AppendLine($"    <p class=\"summary\">{Encode(result.Summary)}</p>");
        html.AppendLine("    <div class=\"grid\">");
        html.AppendLine(RenderMetricCard("Regression Status", result.Status.ToString(), result.Status == AiRegressionStatus.NoRegression ? "good" : "warn"));
        html.AppendLine(RenderMetricCard("Finding Count", result.Findings.Count.ToString(), result.Findings.Count == 0 ? "good" : "warn"));
        html.AppendLine(RenderMetricCard("Baseline Category", result.BaselinePrimaryCategory, "base"));
        html.AppendLine(RenderMetricCard("Candidate Category", result.CandidatePrimaryCategory, "candidate"));
        html.AppendLine("    </div>");
        html.AppendLine("    <div class=\"bundle-grid\">");
        html.AppendLine(RenderBundleCard("Baseline Run", baselineBundle, false));
        html.AppendLine(RenderBundleCard("Candidate Run", candidateBundle, true));
        html.AppendLine("    </div>");
        html.AppendLine("  </section>");
        html.AppendLine("  <section class=\"stack\">");
        html.AppendLine(RenderMetadataSection(result));
        html.AppendLine(RenderFindingsSection(result));
        html.AppendLine(RenderComparedFactsSection(baselineBundle, candidateBundle));
        html.AppendLine("  </section>");
        html.AppendLine("  <p class=\"footer\">This regression report is deterministic, bundle-driven, and fully offline. No network calls, provider integration, or execution-engine access are required.</p>");
        html.AppendLine("</main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string RenderMetricCard(string label, string value, string chipStyle)
    {
        return $"      <div class=\"card\"><span class=\"label\">{Encode(label)}</span><div class=\"value\">{Encode(Placeholder(value))}</div><span class=\"chip {chipStyle}\">{Encode(Placeholder(value))}</span></div>";
    }

    private static string RenderBundleCard(string title, AiAnalysisBundle bundle, bool isCandidate)
    {
        var html = new StringBuilder();
        html.AppendLine($"      <article class=\"card bundle-card{(isCandidate ? " candidate" : string.Empty)}\">");
        html.AppendLine($"        <span class=\"chip {(isCandidate ? "candidate" : "base")}\">{Encode(title)}</span>");
        html.AppendLine($"        <h2>{Encode(Placeholder(bundle.Analysis.PrimaryCategory))}</h2>");
        html.AppendLine($"        <p>{Encode(Placeholder(bundle.Analysis.PrimaryCause))}</p>");
        html.AppendLine("        <div class=\"grid\">");
        html.AppendLine(RenderBundleValue("Run Status", bundle.Summary.RunStatus));
        html.AppendLine(RenderBundleValue("Failed Specs", bundle.Summary.FailedSpecCount.ToString()));
        html.AppendLine(RenderBundleValue("Exceptions", bundle.Summary.ExceptionCount.ToString()));
        html.AppendLine(RenderBundleValue("Variable Failures", bundle.Summary.VariableResolutionFailedCount.ToString()));
        html.AppendLine("        </div>");
        html.AppendLine($"        <p><strong>Session:</strong> {Encode(Placeholder(bundle.Summary.SessionId))}</p>");
        html.AppendLine($"        <p><strong>Recipe:</strong> {Encode(Placeholder(bundle.Summary.RecipeName))}</p>");
        html.AppendLine("      </article>");
        return html.ToString();
    }

    private static string RenderBundleValue(string label, string value)
    {
        return $"          <div><span class=\"label\">{Encode(label)}</span><div class=\"value small\">{Encode(Placeholder(value))}</div></div>";
    }

    private static string RenderMetadataSection(AiRegressionCheckResult result)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine("      <summary><span>Regression Metadata</span><span class=\"summary-meta\">Output contract and artifact paths</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <table>");
        html.AppendLine(RenderRow("Schema Version", result.SchemaVersion));
        html.AppendLine(RenderRow("Generated At (UTC)", result.GeneratedAtUtc == default ? string.Empty : result.GeneratedAtUtc.ToString("u")));
        html.AppendLine(RenderRow("Baseline Bundle Path", result.BaselineBundlePath, true));
        html.AppendLine(RenderRow("Candidate Bundle Path", result.CandidateBundlePath, true));
        html.AppendLine("        </table>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderFindingsSection(AiRegressionCheckResult result)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine($"      <summary><span>Findings</span><span class=\"summary-meta\">{result.Findings.Count} regression findings</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");

        if (result.Findings.Count == 0)
        {
            html.AppendLine("        <p class=\"empty\">No regression findings were detected.</p>");
        }
        else
        {
            foreach (var finding in result.Findings)
            {
                html.AppendLine("        <article class=\"finding\">");
                html.AppendLine($"          <div class=\"meta\">Code: <code>{Encode(Placeholder(finding.Code))}</code> | Source: <code>{Encode(Placeholder(finding.Source))}</code></div>");
                html.AppendLine($"          <h3>{Encode(Placeholder(finding.Title))}</h3>");
                html.AppendLine($"          <p>{Encode(Placeholder(finding.Message))}</p>");
                html.AppendLine("          <table>");
                html.AppendLine("            <tbody>");
                html.AppendLine(RenderRow("Baseline", finding.BaselineValue));
                html.AppendLine(RenderRow("Candidate", finding.CandidateValue));
                html.AppendLine("            </tbody>");
                html.AppendLine("          </table>");
                html.AppendLine("        </article>");
            }
        }

        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderComparedFactsSection(AiAnalysisBundle baselineBundle, AiAnalysisBundle candidateBundle)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine("      <summary><span>Compared Facts</span><span class=\"summary-meta\">Key deterministic baseline vs candidate fields</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <table>");
        html.AppendLine("          <thead><tr><th>Field</th><th>Baseline</th><th>Candidate</th></tr></thead>");
        html.AppendLine("          <tbody>");
        html.AppendLine(RenderComparisonRow("Primary Category", baselineBundle.Analysis.PrimaryCategory, candidateBundle.Analysis.PrimaryCategory));
        html.AppendLine(RenderComparisonRow("Primary Cause", baselineBundle.Analysis.PrimaryCause, candidateBundle.Analysis.PrimaryCause));
        html.AppendLine(RenderComparisonRow("Failed Spec Count", baselineBundle.Summary.FailedSpecCount.ToString(), candidateBundle.Summary.FailedSpecCount.ToString()));
        html.AppendLine(RenderComparisonRow("Exception Count", baselineBundle.Summary.ExceptionCount.ToString(), candidateBundle.Summary.ExceptionCount.ToString()));
        html.AppendLine(RenderComparisonRow("Variable Resolution Failed Count", baselineBundle.Summary.VariableResolutionFailedCount.ToString(), candidateBundle.Summary.VariableResolutionFailedCount.ToString()));
        html.AppendLine(RenderComparisonRow("Failed Step Names", JoinOrPlaceholder(baselineBundle.Summary.FailedStepNames), JoinOrPlaceholder(candidateBundle.Summary.FailedStepNames)));
        html.AppendLine(RenderComparisonRow("Matched Rules", JoinOrPlaceholder(baselineBundle.Analysis.MatchedRules), JoinOrPlaceholder(candidateBundle.Analysis.MatchedRules)));
        html.AppendLine("          </tbody>");
        html.AppendLine("        </table>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderRow(string label, string value, bool code = false)
    {
        var renderedValue = code
            ? $"<code>{Encode(Placeholder(value))}</code>"
            : Encode(Placeholder(value));
        return $"          <tr><th>{Encode(label)}</th><td>{renderedValue}</td></tr>";
    }

    private static string RenderComparisonRow(string label, string baselineValue, string candidateValue)
    {
        return $"            <tr><th>{Encode(label)}</th><td>{Encode(Placeholder(baselineValue))}</td><td>{Encode(Placeholder(candidateValue))}</td></tr>";
    }

    private static string JoinOrPlaceholder(IReadOnlyList<string> values)
    {
        return values.Count == 0
            ? "N/A"
            : string.Join(", ", values);
    }

    private static string Placeholder(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "N/A"
            : value;
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
