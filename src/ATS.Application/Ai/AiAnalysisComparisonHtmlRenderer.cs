using System.Net;
using System.Text;
using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiAnalysisComparisonHtmlRenderer
{
    public string Render(AiAnalysisBundleComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine("  <title>ATS Analysis Comparison Viewer</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    :root { --bg: #f4efe7; --card: #fffdf9; --ink: #201c17; --muted: #70675f; --line: #ddd3c4; --left: #145b8f; --left-soft: #e0efff; --right: #7f3a0e; --right-soft: #ffe8d7; --good: #0e6a5c; --good-soft: #d8f0ea; --warn: #a06a00; --warn-soft: #fff2d6; --shadow: rgba(45, 35, 20, 0.08); }");
        html.AppendLine("    * { box-sizing: border-box; }");
        html.AppendLine("    body { margin: 0; font-family: \"Segoe UI\", \"Noto Sans\", sans-serif; color: var(--ink); background: linear-gradient(180deg, #ece2d2 0%, var(--bg) 100%); }");
        html.AppendLine("    main { max-width: 1180px; margin: 0 auto; padding: 28px 20px 40px; }");
        html.AppendLine("    .hero, details, .card { background: var(--card); border: 1px solid var(--line); border-radius: 18px; box-shadow: 0 12px 32px var(--shadow); }");
        html.AppendLine("    .hero { padding: 24px; }");
        html.AppendLine("    .eyebrow { margin: 0 0 10px; text-transform: uppercase; letter-spacing: 0.14em; font-size: 12px; color: var(--muted); }");
        html.AppendLine("    h1, h2, h3, p { margin-top: 0; }");
        html.AppendLine("    h1 { margin-bottom: 12px; font-size: 34px; }");
        html.AppendLine("    h2 { margin-bottom: 10px; font-size: 22px; }");
        html.AppendLine("    .summary { font-size: 18px; max-width: 74ch; }");
        html.AppendLine("    .grid, .bundle-grid, .change-grid { display: grid; gap: 14px; margin-top: 18px; }");
        html.AppendLine("    .grid { grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); }");
        html.AppendLine("    .bundle-grid { grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); }");
        html.AppendLine("    .change-grid { grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); }");
        html.AppendLine("    .card { padding: 16px; }");
        html.AppendLine("    .bundle-card { border-top: 6px solid var(--left); }");
        html.AppendLine("    .bundle-card.right { border-top-color: var(--right); }");
        html.AppendLine("    .label { display: block; margin-bottom: 8px; color: var(--muted); font-size: 12px; letter-spacing: 0.08em; text-transform: uppercase; }");
        html.AppendLine("    .value { font-size: 24px; font-weight: 700; }");
        html.AppendLine("    .value.small { font-size: 18px; }");
        html.AppendLine("    .chip { display: inline-block; padding: 6px 10px; border-radius: 999px; font-size: 12px; font-weight: 700; }");
        html.AppendLine("    .chip.left { background: var(--left-soft); color: var(--left); }");
        html.AppendLine("    .chip.right { background: var(--right-soft); color: var(--right); }");
        html.AppendLine("    .chip.same { background: var(--good-soft); color: var(--good); }");
        html.AppendLine("    .chip.changed { background: var(--warn-soft); color: var(--warn); }");
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
        html.AppendLine("    .empty { color: var(--muted); font-style: italic; }");
        html.AppendLine("    .delta-pos { color: var(--right); font-weight: 700; }");
        html.AppendLine("    .delta-neg { color: var(--left); font-weight: 700; }");
        html.AppendLine("    code { font-family: Consolas, \"Courier New\", monospace; font-size: 13px; }");
        html.AppendLine("    .footer { margin-top: 18px; color: var(--muted); font-size: 14px; }");
        html.AppendLine("    @media (max-width: 720px) { h1 { font-size: 28px; } .value { font-size: 20px; } main { padding: 18px 12px 28px; } th, td { display: block; width: 100%; } }");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<main>");
        html.AppendLine("  <section class=\"hero\">");
        html.AppendLine("    <p class=\"eyebrow\">ATS Offline Multi-Run Comparison Viewer</p>");
        html.AppendLine("    <h1>Analysis Bundle Comparison</h1>");
        html.AppendLine($"    <p class=\"summary\">{Encode(BuildSummary(comparison))}</p>");
        html.AppendLine("    <div class=\"grid\">");
        html.AppendLine(RenderMetricCard("Differences Detected", comparison.HasDifferences ? "Yes" : "No"));
        html.AppendLine(RenderMetricCard("Changed Summary Counts", comparison.SummaryCountChanges.Count.ToString()));
        html.AppendLine(RenderMetricCard("Changed Rules", (comparison.AddedMatchedRules.Count + comparison.RemovedMatchedRules.Count).ToString()));
        html.AppendLine(RenderMetricCard("Changed Evidence", (comparison.AddedEvidence.Count + comparison.RemovedEvidence.Count).ToString()));
        html.AppendLine(RenderMetricCard("Changed Actions", (comparison.AddedRecommendedActions.Count + comparison.RemovedRecommendedActions.Count).ToString()));
        html.AppendLine("    </div>");
        html.AppendLine("    <div class=\"bundle-grid\">");
        html.AppendLine(RenderBundleCard("Left Bundle", comparison.LeftBundle, false));
        html.AppendLine(RenderBundleCard("Right Bundle", comparison.RightBundle, true));
        html.AppendLine("    </div>");
        html.AppendLine("  </section>");
        html.AppendLine("  <section class=\"stack\">");
        html.AppendLine(RenderMetadataSection(comparison));
        html.AppendLine(RenderFieldChangesSection(comparison));
        html.AppendLine(RenderSummaryCountChangesSection(comparison.SummaryCountChanges));
        html.AppendLine(RenderStringDeltaSection("Failed Step Name Changes", comparison.AddedFailedStepNames, comparison.RemovedFailedStepNames, "No failed step name changes were detected."));
        html.AppendLine(RenderStringDeltaSection("Matched Rule Changes", comparison.AddedMatchedRules, comparison.RemovedMatchedRules, "No matched rule changes were detected."));
        html.AppendLine(RenderEvidenceSection(comparison));
        html.AppendLine(RenderStringDeltaSection("Recommended Action Changes", comparison.AddedRecommendedActions, comparison.RemovedRecommendedActions, "No recommended action changes were detected."));
        html.AppendLine("  </section>");
        html.AppendLine("  <p class=\"footer\">This comparison report is bundle-driven and fully offline. No web server, live UI, or network calls are required.</p>");
        html.AppendLine("</main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string RenderMetricCard(string label, string value)
    {
        return $"      <div class=\"card\"><span class=\"label\">{Encode(label)}</span><div class=\"value\">{Encode(Placeholder(value))}</div></div>";
    }

    private static string RenderBundleCard(string title, AiAnalysisBundle bundle, bool isRight)
    {
        var html = new StringBuilder();
        html.AppendLine($"      <article class=\"card bundle-card{(isRight ? " right" : string.Empty)}\">");
        html.AppendLine($"        <span class=\"chip {(isRight ? "right" : "left")}\">{Encode(title)}</span>");
        html.AppendLine($"        <h2>{Encode(Placeholder(bundle.Analysis.PrimaryCategory))}</h2>");
        html.AppendLine($"        <p>{Encode(Placeholder(bundle.Analysis.PrimaryCause))}</p>");
        html.AppendLine("        <div class=\"change-grid\">");
        html.AppendLine(RenderBundleValue("Confidence", bundle.Analysis.Confidence?.ToString("0.00") ?? "N/A"));
        html.AppendLine(RenderBundleValue("Run Status", bundle.Summary.RunStatus));
        html.AppendLine(RenderBundleValue("Failed Steps", bundle.Summary.FailedStepCount.ToString()));
        html.AppendLine(RenderBundleValue("Matched Rules", bundle.Analysis.MatchedRules.Count.ToString()));
        html.AppendLine("        </div>");
        html.AppendLine($"        <p><strong>Recipe:</strong> {Encode(Placeholder(bundle.Summary.RecipeName))}</p>");
        html.AppendLine($"        <p><strong>Session:</strong> {Encode(Placeholder(bundle.Summary.SessionId))}</p>");
        html.AppendLine($"        <p><strong>Result JSON:</strong> <code>{Encode(Placeholder(bundle.ResultJsonPath))}</code></p>");
        html.AppendLine("      </article>");
        return html.ToString();
    }

    private static string RenderBundleValue(string label, string value)
    {
        return $"          <div><span class=\"label\">{Encode(label)}</span><div class=\"value small\">{Encode(Placeholder(value))}</div></div>";
    }

    private static string RenderMetadataSection(AiAnalysisBundleComparison comparison)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine("      <summary><span>Bundle Metadata</span><span class=\"summary-meta\">Artifact sources and schema details</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <table>");
        html.AppendLine(RenderComparisonRow("Schema Version", comparison.LeftBundle.SchemaVersion, comparison.RightBundle.SchemaVersion));
        html.AppendLine(RenderComparisonRow("Analyzer", comparison.LeftBundle.AnalyzerName, comparison.RightBundle.AnalyzerName));
        html.AppendLine(RenderComparisonRow("Generated At (UTC)", FormatDate(comparison.LeftBundle.GeneratedAtUtc), FormatDate(comparison.RightBundle.GeneratedAtUtc)));
        html.AppendLine(RenderComparisonRow("Result JSON Path", comparison.LeftBundle.ResultJsonPath, comparison.RightBundle.ResultJsonPath, true));
        html.AppendLine(RenderComparisonRow("Events JSONL Path", comparison.LeftBundle.EventsJsonlPath, comparison.RightBundle.EventsJsonlPath, true));
        html.AppendLine(RenderComparisonRow("Analysis JSON Path", comparison.LeftBundle.AnalysisJsonPath, comparison.RightBundle.AnalysisJsonPath, true));
        html.AppendLine("        </table>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderFieldChangesSection(AiAnalysisBundleComparison comparison)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine("      <summary><span>Analysis Field Changes</span><span class=\"summary-meta\">Primary category, cause, and confidence</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <table>");
        html.AppendLine("          <thead><tr><th>Field</th><th>Left</th><th>Right</th><th>Status</th></tr></thead>");
        html.AppendLine("          <tbody>");
        html.AppendLine(RenderValueChangeRow(comparison.PrimaryCategory));
        html.AppendLine(RenderValueChangeRow(comparison.PrimaryCause));
        html.AppendLine(RenderValueChangeRow(comparison.Confidence));
        html.AppendLine("          </tbody>");
        html.AppendLine("        </table>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderSummaryCountChangesSection(IReadOnlyList<AiComparisonCountChange> changes)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine($"      <summary><span>Summary Count Changes</span><span class=\"summary-meta\">{changes.Count} changed counters</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");

        if (changes.Count == 0)
        {
            html.AppendLine("        <p class=\"empty\">No normalized summary count changes were detected.</p>");
        }
        else
        {
            html.AppendLine("        <table>");
            html.AppendLine("          <thead><tr><th>Field</th><th>Left</th><th>Right</th><th>Delta</th></tr></thead>");
            html.AppendLine("          <tbody>");
            foreach (var change in changes)
            {
                var deltaClass = change.Delta > 0 ? "delta-pos" : "delta-neg";
                var deltaText = change.Delta > 0 ? $"+{change.Delta}" : change.Delta.ToString();
                html.AppendLine($"            <tr><th>{Encode(change.Label)}</th><td>{change.LeftValue}</td><td>{change.RightValue}</td><td class=\"{deltaClass}\">{Encode(deltaText)}</td></tr>");
            }

            html.AppendLine("          </tbody>");
            html.AppendLine("        </table>");
        }

        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderStringDeltaSection(string title, IReadOnlyList<string> addedValues, IReadOnlyList<string> removedValues, string emptyMessage)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine($"      <summary><span>{Encode(title)}</span><span class=\"summary-meta\">added {addedValues.Count} / removed {removedValues.Count}</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <div class=\"bundle-grid\">");
        html.AppendLine(RenderStringCard("Added", addedValues, "right"));
        html.AppendLine(RenderStringCard("Removed", removedValues, "left"));
        html.AppendLine("        </div>");

        if (addedValues.Count == 0 && removedValues.Count == 0)
        {
            html.AppendLine($"        <p class=\"empty\">{Encode(emptyMessage)}</p>");
        }

        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderStringCard(string title, IReadOnlyList<string> values, string chipStyle)
    {
        var html = new StringBuilder();
        html.AppendLine("          <article class=\"card\">");
        html.AppendLine($"            <span class=\"chip {chipStyle}\">{Encode(title)}</span>");

        if (values.Count == 0)
        {
            html.AppendLine("            <p class=\"empty\">No items.</p>");
        }
        else
        {
            html.AppendLine("            <ul class=\"list\">");
            foreach (var value in values)
            {
                html.AppendLine($"              <li>{Encode(value)}</li>");
            }

            html.AppendLine("            </ul>");
        }

        html.AppendLine("          </article>");
        return html.ToString();
    }

    private static string RenderEvidenceSection(AiAnalysisBundleComparison comparison)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details open>");
        html.AppendLine($"      <summary><span>Evidence Changes</span><span class=\"summary-meta\">added {comparison.AddedEvidence.Count} / removed {comparison.RemovedEvidence.Count}</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <div class=\"bundle-grid\">");
        html.AppendLine(RenderEvidenceCard("Added Evidence", comparison.AddedEvidence, "right"));
        html.AppendLine(RenderEvidenceCard("Removed Evidence", comparison.RemovedEvidence, "left"));
        html.AppendLine("        </div>");

        if (comparison.AddedEvidence.Count == 0 && comparison.RemovedEvidence.Count == 0)
        {
            html.AppendLine("        <p class=\"empty\">No evidence changes were detected.</p>");
        }

        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderEvidenceCard(string title, IReadOnlyList<AiEvidenceItem> values, string chipStyle)
    {
        var html = new StringBuilder();
        html.AppendLine("          <article class=\"card\">");
        html.AppendLine($"            <span class=\"chip {chipStyle}\">{Encode(title)}</span>");

        if (values.Count == 0)
        {
            html.AppendLine("            <p class=\"empty\">No items.</p>");
        }
        else
        {
            html.AppendLine("            <table>");
            html.AppendLine("              <thead><tr><th>Type</th><th>Message</th><th>Source</th><th>Value</th></tr></thead>");
            html.AppendLine("              <tbody>");
            foreach (var value in values)
            {
                html.AppendLine("                <tr>");
                html.AppendLine($"                  <td>{Encode(Placeholder(value.Type))}</td>");
                html.AppendLine($"                  <td>{Encode(Placeholder(value.Message))}</td>");
                html.AppendLine($"                  <td><code>{Encode(Placeholder(value.Source))}</code></td>");
                html.AppendLine($"                  <td>{Encode(Placeholder(value.Value))}</td>");
                html.AppendLine("                </tr>");
            }

            html.AppendLine("              </tbody>");
            html.AppendLine("            </table>");
        }

        html.AppendLine("          </article>");
        return html.ToString();
    }

    private static string RenderComparisonRow(string label, string leftValue, string rightValue, bool code = false)
    {
        return $"          <tr><th>{Encode(label)}</th><td>{RenderCell(leftValue, code)}</td><td>{RenderCell(rightValue, code)}</td></tr>";
    }

    private static string RenderValueChangeRow(AiComparisonValueChange change)
    {
        var statusClass = change.Changed ? "changed" : "same";
        var statusText = change.Changed ? "Changed" : "Unchanged";
        return $"            <tr><th>{Encode(change.Label)}</th><td>{Encode(Placeholder(change.LeftValue))}</td><td>{Encode(Placeholder(change.RightValue))}</td><td><span class=\"chip {statusClass}\">{statusText}</span></td></tr>";
    }

    private static string RenderCell(string value, bool code)
    {
        var encoded = Encode(Placeholder(value));
        return code ? $"<code>{encoded}</code>" : encoded;
    }

    private static string BuildSummary(AiAnalysisBundleComparison comparison)
    {
        return comparison.HasDifferences
            ? $"Detected {CountDifferences(comparison)} changed comparison areas between the left and right analysis bundles."
            : "No differences were detected between the two analysis bundles.";
    }

    private static int CountDifferences(AiAnalysisBundleComparison comparison)
    {
        var count = 0;
        count += comparison.PrimaryCategory.Changed ? 1 : 0;
        count += comparison.PrimaryCause.Changed ? 1 : 0;
        count += comparison.Confidence.Changed ? 1 : 0;
        count += comparison.SummaryCountChanges.Count;
        count += comparison.AddedMatchedRules.Count + comparison.RemovedMatchedRules.Count;
        count += comparison.AddedFailedStepNames.Count + comparison.RemovedFailedStepNames.Count;
        count += comparison.AddedRecommendedActions.Count + comparison.RemovedRecommendedActions.Count;
        count += comparison.AddedEvidence.Count + comparison.RemovedEvidence.Count;
        return count;
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return value == default ? string.Empty : value.ToString("u");
    }

    private static string Placeholder(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "N/A" : value;
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
