using System.Net;
using System.Text;
using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiAnalysisHtmlRenderer
{
    public string Render(AiAnalysisBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine("  <title>ATS Analysis Viewer</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    :root { color-scheme: light; --bg: #f5f1e8; --card: #fffdf9; --ink: #1c1a17; --muted: #6f6a63; --line: #dfd7ca; --accent: #0c6d62; --accent-soft: #dff2ee; --warn: #9a6700; --warn-soft: #fff1d6; --error: #a33a1d; --error-soft: #ffe3db; --shadow: rgba(53, 40, 23, 0.08); }");
        html.AppendLine("    * { box-sizing: border-box; }");
        html.AppendLine("    body { margin: 0; font-family: \"Segoe UI\", \"Noto Sans\", sans-serif; background: linear-gradient(180deg, #efe5d3 0%, var(--bg) 100%); color: var(--ink); }");
        html.AppendLine("    main { max-width: 1100px; margin: 0 auto; padding: 28px 20px 40px; }");
        html.AppendLine("    .hero { background: radial-gradient(circle at top right, #f8dbc2 0%, #fffdf9 55%, #f3eee4 100%); border: 1px solid var(--line); border-radius: 20px; padding: 24px; box-shadow: 0 18px 44px var(--shadow); }");
        html.AppendLine("    .eyebrow { text-transform: uppercase; letter-spacing: 0.14em; font-size: 12px; color: var(--muted); margin: 0 0 10px; }");
        html.AppendLine("    h1, h2 { margin: 0 0 12px; line-height: 1.15; }");
        html.AppendLine("    h1 { font-size: 34px; }");
        html.AppendLine("    h2 { font-size: 20px; }");
        html.AppendLine("    p { margin: 0 0 10px; }");
        html.AppendLine("    .summary { font-size: 18px; max-width: 72ch; }");
        html.AppendLine("    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(210px, 1fr)); gap: 14px; margin-top: 18px; }");
        html.AppendLine("    .count-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; margin-top: 14px; }");
        html.AppendLine("    .card { background: var(--card); border: 1px solid var(--line); border-radius: 16px; padding: 16px; box-shadow: 0 8px 24px rgba(53, 40, 23, 0.05); }");
        html.AppendLine("    .label { display: block; font-size: 12px; text-transform: uppercase; letter-spacing: 0.08em; color: var(--muted); margin-bottom: 8px; }");
        html.AppendLine("    .value { font-size: 24px; font-weight: 700; }");
        html.AppendLine("    .value.small { font-size: 18px; }");
        html.AppendLine("    .stack { display: grid; gap: 14px; margin-top: 22px; }");
        html.AppendLine("    .controls { display: grid; gap: 12px; }");
        html.AppendLine("    .controls-grid { display: grid; grid-template-columns: minmax(260px, 1.7fr) repeat(2, minmax(180px, 1fr)); gap: 12px; align-items: end; }");
        html.AppendLine("    .control label { display: block; font-size: 12px; text-transform: uppercase; letter-spacing: 0.08em; color: var(--muted); margin-bottom: 6px; }");
        html.AppendLine("    .control input, .control select { width: 100%; border: 1px solid var(--line); border-radius: 12px; padding: 10px 12px; font: inherit; background: #fff; color: var(--ink); }");
        html.AppendLine("    .helper { display: flex; flex-wrap: wrap; gap: 10px; align-items: center; color: var(--muted); font-size: 14px; }");
        html.AppendLine("    .helper strong { color: var(--ink); }");
        html.AppendLine("    .stack > details, .stack > article { background: var(--card); border: 1px solid var(--line); border-radius: 16px; box-shadow: 0 8px 24px rgba(53, 40, 23, 0.05); }");
        html.AppendLine("    details > summary { list-style: none; cursor: pointer; padding: 16px; display: flex; justify-content: space-between; align-items: center; gap: 12px; font-weight: 700; }");
        html.AppendLine("    details > summary::-webkit-details-marker { display: none; }");
        html.AppendLine("    details[open] > summary { border-bottom: 1px solid var(--line); }");
        html.AppendLine("    .summary-meta { color: var(--muted); font-size: 13px; font-weight: 600; }");
        html.AppendLine("    .section-body { padding: 16px; }");
        html.AppendLine("    .list { margin: 0; padding-left: 20px; }");
        html.AppendLine("    .list li { margin-bottom: 8px; }");
        html.AppendLine("    .pill { display: inline-block; padding: 6px 10px; border-radius: 999px; background: var(--accent-soft); color: var(--accent); font-weight: 600; font-size: 13px; margin-right: 8px; margin-bottom: 8px; }");
        html.AppendLine("    .observation { border-left: 4px solid var(--accent); padding: 12px 14px; background: #f6fcfa; border-radius: 12px; margin-bottom: 10px; }");
        html.AppendLine("    .observation.warning { border-left-color: var(--warn); background: var(--warn-soft); }");
        html.AppendLine("    .observation.error { border-left-color: var(--error); background: var(--error-soft); }");
        html.AppendLine("    .observation .title { font-weight: 700; margin-bottom: 6px; }");
        html.AppendLine("    .observation .meta { color: var(--muted); font-size: 13px; margin-bottom: 4px; }");
        html.AppendLine("    table { width: 100%; border-collapse: collapse; }");
        html.AppendLine("    th, td { text-align: left; vertical-align: top; border-bottom: 1px solid var(--line); padding: 10px 12px; font-size: 14px; }");
        html.AppendLine("    th { color: var(--muted); font-weight: 600; width: 28%; }");
        html.AppendLine("    code { font-family: Consolas, \"Courier New\", monospace; font-size: 13px; }");
        html.AppendLine("    .muted { color: var(--muted); }");
        html.AppendLine("    .no-results { color: var(--muted); font-style: italic; margin-top: 6px; }");
        html.AppendLine("    .js-only { display: none; }");
        html.AppendLine("    body.js-enabled .js-only { display: inline-flex; }");
        html.AppendLine("    body.js-enabled .js-helper { display: block; }");
        html.AppendLine("    .footer-note { margin-top: 18px; color: var(--muted); font-size: 14px; }");
        html.AppendLine("    @media (max-width: 860px) { .controls-grid { grid-template-columns: 1fr; } }");
        html.AppendLine("    @media (max-width: 720px) { h1 { font-size: 28px; } .value { font-size: 20px; } main { padding: 18px 12px 28px; } th, td { display: block; width: 100%; } th { padding-bottom: 2px; border-bottom: none; } td { padding-top: 0; } details > summary { flex-direction: column; align-items: flex-start; } }");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<main>");
        html.AppendLine("  <section class=\"hero\">");
        html.AppendLine("    <p class=\"eyebrow\">ATS Interactive Offline Analysis Viewer</p>");
        html.AppendLine("    <h1>Analysis Bundle Viewer</h1>");
        html.AppendLine($"    <p class=\"summary\">{Encode(bundle.Analysis.Summary)}</p>");
        html.AppendLine("    <div class=\"grid\">");
        html.AppendLine(RenderMetricCard("Primary Category", bundle.Analysis.PrimaryCategory));
        html.AppendLine(RenderMetricCard("Primary Cause", bundle.Analysis.PrimaryCause));
        html.AppendLine(RenderMetricCard("Confidence", bundle.Analysis.Confidence?.ToString("0.00") ?? "N/A"));
        html.AppendLine(RenderMetricCard("Analyzer", bundle.AnalyzerName));
        html.AppendLine("    </div>");
        html.AppendLine("    <div class=\"count-grid\">");
        html.AppendLine(RenderCountCard("Step Count", bundle.Summary.StepCount.ToString()));
        html.AppendLine(RenderCountCard("Failed Steps", bundle.Summary.FailedStepCount.ToString()));
        html.AppendLine(RenderCountCard("Variable Failures", bundle.Summary.VariableResolutionFailedCount.ToString()));
        html.AppendLine(RenderCountCard("Exceptions", bundle.Summary.ExceptionCount.ToString()));
        html.AppendLine(RenderCountCard("Warnings", bundle.Summary.WarningCount.ToString()));
        html.AppendLine(RenderCountCard("Failed Specs", bundle.Summary.FailedSpecCount.ToString()));
        html.AppendLine("    </div>");
        html.AppendLine("  </section>");
        html.AppendLine("  <section class=\"stack\">");
        html.AppendLine("    <article class=\"card controls\">");
        html.AppendLine("      <h2>Interactive Controls</h2>");
        html.AppendLine("      <div class=\"controls-grid\">");
        html.AppendLine("        <div class=\"control\">");
        html.AppendLine("          <label for=\"viewer-search\">Search</label>");
        html.AppendLine("          <input id=\"viewer-search\" type=\"search\" placeholder=\"Search messages, rules, evidence, and step names\" data-viewer-search>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class=\"control\">");
        html.AppendLine("          <label for=\"viewer-severity-filter\">Severity Filter</label>");
        html.AppendLine("          <select id=\"viewer-severity-filter\" data-viewer-severity-filter>");
        html.AppendLine("            <option value=\"all\">All Severities</option>");
        html.AppendLine("            <option value=\"error\">Error</option>");
        html.AppendLine("            <option value=\"warning\">Warning</option>");
        html.AppendLine("            <option value=\"info\">Info</option>");
        html.AppendLine("          </select>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class=\"control\">");
        html.AppendLine("          <label for=\"viewer-category-filter\">Category Filter</label>");
        html.AppendLine("          <select id=\"viewer-category-filter\" data-viewer-category-filter>");
        html.AppendLine("            <option value=\"all\">All Categories</option>");
        html.AppendLine("            <option value=\"observations\">Observations</option>");
        html.AppendLine("            <option value=\"recommended-actions\">Recommended Actions</option>");
        html.AppendLine("            <option value=\"evidence\">Evidence</option>");
        html.AppendLine("            <option value=\"matched-rules\">Matched Rules</option>");
        html.AppendLine("            <option value=\"summary-facts\">Summary Facts</option>");
        html.AppendLine("          </select>");
        html.AppendLine("        </div>");
        html.AppendLine("      </div>");
        html.AppendLine("      <div class=\"helper\">");
        html.AppendLine("        <span>Use search and filters to narrow the report without leaving the page.</span>");
        html.AppendLine("        <span class=\"pill js-only\"><strong data-viewer-match-count>0</strong>&nbsp;matches</span>");
        html.AppendLine("      </div>");
        html.AppendLine("      <noscript><p class=\"muted\">JavaScript is optional. All report content remains visible below even if search and filters are unavailable.</p></noscript>");
        html.AppendLine("    </article>");
        html.AppendLine(RenderMetadataSection(bundle));
        html.AppendLine(RenderObservationsSection(bundle.Analysis.Observations));
        html.AppendLine(RenderActionsSection(bundle.Analysis.RecommendedActions));
        html.AppendLine(RenderEvidenceSection(bundle.Analysis.Evidence));
        html.AppendLine(RenderMatchedRulesSection(bundle.Analysis.MatchedRules));
        html.AppendLine(RenderSummaryFactsSection(bundle.Summary));
        html.AppendLine("  </section>");
        html.AppendLine("  <p class=\"footer-note\">This viewer is fully offline and bundle-driven. No web server, external CSS, or network calls are required.</p>");
        html.AppendLine(RenderScript());
        html.AppendLine("</main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string RenderMetricCard(string label, string value)
    {
        return
            $"      <div class=\"card\"><span class=\"label\">{Encode(label)}</span><div class=\"value\">{Encode(EmptyToPlaceholder(value))}</div></div>";
    }

    private static string RenderCountCard(string label, string value)
    {
        return
            $"      <div class=\"card\"><span class=\"label\">{Encode(label)}</span><div class=\"value small\">{Encode(EmptyToPlaceholder(value))}</div></div>";
    }

    private static string RenderMetadataSection(AiAnalysisBundle bundle)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details class=\"section-panel\" open data-search-group data-group-category=\"always\" data-section-title=\"metadata\">");
        html.AppendLine("      <summary><span>Header / Artifact Metadata</span><span class=\"summary-meta\">Bundle contract details</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <table>");
        html.AppendLine(RenderRow("Schema Version", bundle.SchemaVersion, false, "metadata"));
        html.AppendLine(RenderRow("Generated At (UTC)", bundle.GeneratedAtUtc == default ? string.Empty : bundle.GeneratedAtUtc.ToString("u"), false, "metadata"));
        html.AppendLine(RenderRow("Result JSON Path", bundle.ResultJsonPath, true, "metadata"));
        html.AppendLine(RenderRow("Events JSONL Path", bundle.EventsJsonlPath, true, "metadata"));
        html.AppendLine(RenderRow("Analysis JSON Path", bundle.AnalysisJsonPath, true, "metadata"));
        html.AppendLine("        </table>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderObservationsSection(IReadOnlyList<AiObservation> observations)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details class=\"section-panel\" open data-search-group data-group-category=\"observations\" data-section-title=\"observations\">");
        html.AppendLine($"      <summary><span>Observations</span><span class=\"summary-meta\">{observations.Count} items</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");

        if (observations.Count == 0)
        {
            html.AppendLine("        <p class=\"muted\">No observations were recorded.</p>");
        }
        else
        {
            foreach (var observation in observations)
            {
                var severity = NormalizeToken(observation.Severity);
                var severityClass = severity switch
                {
                    "warning" => " warning",
                    "error" => " error",
                    _ => string.Empty
                };
                var searchText = $"{observation.Severity} {observation.Title} {observation.Detail}";
                html.AppendLine(
                    $"        <div class=\"observation{severityClass}\" data-search-item data-content-category=\"observations\" data-severity=\"{EncodeAttribute(severity)}\" data-search-text=\"{EncodeAttribute(searchText)}\">");
                html.AppendLine($"          <div class=\"meta\">Severity: {Encode(EmptyToPlaceholder(observation.Severity))}</div>");
                html.AppendLine($"          <div class=\"title\">{Encode(observation.Title)}</div>");
                html.AppendLine($"          <div>{Encode(observation.Detail)}</div>");
                html.AppendLine("        </div>");
            }
        }

        html.AppendLine("        <p class=\"no-results\" hidden data-empty-state>No matching observations.</p>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderActionsSection(IReadOnlyList<string> actions)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details class=\"section-panel\" open data-search-group data-group-category=\"recommended-actions\" data-section-title=\"recommended-actions\">");
        html.AppendLine($"      <summary><span>Recommended Actions</span><span class=\"summary-meta\">{actions.Count} items</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");

        if (actions.Count == 0)
        {
            html.AppendLine("        <p class=\"muted\">No items were recorded.</p>");
        }
        else
        {
            html.AppendLine("        <ul class=\"list\">");
            foreach (var action in actions)
            {
                html.AppendLine(
                    $"          <li data-search-item data-content-category=\"recommended-actions\" data-search-text=\"{EncodeAttribute(action)}\">{Encode(action)}</li>");
            }

            html.AppendLine("        </ul>");
        }

        html.AppendLine("        <p class=\"no-results\" hidden data-empty-state>No matching recommended actions.</p>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderEvidenceSection(IReadOnlyList<AiEvidenceItem> evidence)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details class=\"section-panel\" open data-search-group data-group-category=\"evidence\" data-section-title=\"evidence\">");
        html.AppendLine($"      <summary><span>Evidence</span><span class=\"summary-meta\">{evidence.Count} items</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");

        if (evidence.Count == 0)
        {
            html.AppendLine("        <p class=\"muted\">No evidence items were recorded.</p>");
        }
        else
        {
            html.AppendLine("        <table>");
            html.AppendLine("          <thead><tr><th>Type</th><th>Message</th><th>Source</th><th>Value</th></tr></thead>");
            html.AppendLine("          <tbody>");
            foreach (var item in evidence)
            {
                var searchText = $"{item.Type} {item.Message} {item.Source} {item.Value}";
                html.AppendLine(
                    $"            <tr data-search-item data-content-category=\"evidence\" data-search-text=\"{EncodeAttribute(searchText)}\">");
                html.AppendLine($"              <td>{Encode(item.Type)}</td>");
                html.AppendLine($"              <td>{Encode(item.Message)}</td>");
                html.AppendLine($"              <td><code>{Encode(EmptyToPlaceholder(item.Source))}</code></td>");
                html.AppendLine($"              <td>{Encode(EmptyToPlaceholder(item.Value))}</td>");
                html.AppendLine("            </tr>");
            }

            html.AppendLine("          </tbody>");
            html.AppendLine("        </table>");
        }

        html.AppendLine("        <p class=\"no-results\" hidden data-empty-state>No matching evidence items.</p>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderMatchedRulesSection(IReadOnlyList<string> values)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details class=\"section-panel\" open data-search-group data-group-category=\"matched-rules\" data-section-title=\"matched-rules\">");
        html.AppendLine($"      <summary><span>Matched Rules</span><span class=\"summary-meta\">{values.Count} items</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");

        if (values.Count == 0)
        {
            html.AppendLine("        <p class=\"muted\">No matched rules were recorded.</p>");
        }
        else
        {
            foreach (var value in values)
            {
                html.AppendLine(
                    $"        <span class=\"pill\" data-search-item data-content-category=\"matched-rules\" data-search-text=\"{EncodeAttribute(value)}\">{Encode(value)}</span>");
            }
        }

        html.AppendLine("        <p class=\"no-results\" hidden data-empty-state>No matching rules.</p>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderSummaryFactsSection(RunArtifactSummary summary)
    {
        var html = new StringBuilder();
        html.AppendLine("    <details class=\"section-panel\" open data-search-group data-group-category=\"summary-facts\" data-section-title=\"summary-facts\">");
        html.AppendLine("      <summary><span>Normalized Summary Facts</span><span class=\"summary-meta\">Structured run facts</span></summary>");
        html.AppendLine("      <div class=\"section-body\">");
        html.AppendLine("        <table>");
        html.AppendLine(RenderRow("Session ID", summary.SessionId, false, "summary-facts"));
        html.AppendLine(RenderRow("Command", summary.CommandName, false, "summary-facts"));
        html.AppendLine(RenderRow("Recipe", summary.RecipeName, false, "summary-facts"));
        html.AppendLine(RenderRow("Device", summary.DeviceName, false, "summary-facts"));
        html.AppendLine(RenderRow("Run Status", summary.RunStatus, false, "summary-facts"));
        html.AppendLine(RenderRow("Serial Number", summary.SerialNumber, false, "summary-facts"));
        html.AppendLine(RenderRow("Station", summary.Station, false, "summary-facts"));
        html.AppendLine(RenderRow("Mode", summary.Mode, false, "summary-facts"));
        html.AppendLine(RenderRow("Duration Seconds", summary.DurationSeconds.ToString("0.###"), false, "summary-facts"));
        html.AppendLine(RenderRow("Step Count", summary.StepCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Failed Step Count", summary.FailedStepCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Error Step Count", summary.ErrorStepCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Measurement Count", summary.MeasurementCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Spec Count", summary.SpecCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Failed Spec Count", summary.FailedSpecCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Error Count", summary.ErrorCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Variable Resolved Count", summary.VariableResolvedCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Variable Resolution Failed Count", summary.VariableResolutionFailedCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Exception Count", summary.ExceptionCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Warning Count", summary.WarningCount.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Failed Step Names", JoinOrEmpty(summary.FailedStepNames), false, "summary-facts"));
        html.AppendLine(RenderRow("Error Step Names", JoinOrEmpty(summary.ErrorStepNames), false, "summary-facts"));
        html.AppendLine(RenderRow("Failed Rule Names", JoinOrEmpty(summary.FailedRuleNames), false, "summary-facts"));
        html.AppendLine(RenderRow("Failed Target Keys", JoinOrEmpty(summary.FailedTargetKeys), false, "summary-facts"));
        html.AppendLine(RenderRow("Error Messages", JoinOrEmpty(summary.ErrorMessages), false, "summary-facts"));
        html.AppendLine(RenderRow("First Failure Message", summary.FirstFailureMessage, false, "summary-facts"));
        html.AppendLine(RenderRow("First Exception Message", summary.FirstExceptionMessage, false, "summary-facts"));
        html.AppendLine(RenderRow("Has Variable Resolution Failures", summary.HasVariableResolutionFailures.ToString(), false, "summary-facts"));
        html.AppendLine(RenderRow("Has Unhandled Exception", summary.HasUnhandledException.ToString(), false, "summary-facts"));
        html.AppendLine("        </table>");
        html.AppendLine("        <p class=\"no-results\" hidden data-empty-state>No matching summary facts.</p>");
        html.AppendLine("      </div>");
        html.AppendLine("    </details>");
        return html.ToString();
    }

    private static string RenderRow(string label, string value, bool code, string contentCategory)
    {
        var renderedValue = code
            ? $"<code>{Encode(EmptyToPlaceholder(value))}</code>"
            : Encode(EmptyToPlaceholder(value));
        var searchText = $"{label} {EmptyToPlaceholder(value)}";
        return
            $"          <tr data-search-item data-content-category=\"{EncodeAttribute(contentCategory)}\" data-search-text=\"{EncodeAttribute(searchText)}\"><th>{Encode(label)}</th><td>{renderedValue}</td></tr>";
    }

    private static string RenderScript()
    {
        return
"""
<script>
  (function () {
    document.body.classList.add('js-enabled');

    var searchInput = document.querySelector('[data-viewer-search]');
    var severityFilter = document.querySelector('[data-viewer-severity-filter]');
    var categoryFilter = document.querySelector('[data-viewer-category-filter]');
    var matchCount = document.querySelector('[data-viewer-match-count]');

    function normalize(value) {
      return (value || '').toLowerCase();
    }

    function applyViewerFilters() {
      var searchTerm = normalize(searchInput && searchInput.value);
      var severity = normalize(severityFilter && severityFilter.value) || 'all';
      var category = normalize(categoryFilter && categoryFilter.value) || 'all';
      var items = Array.prototype.slice.call(document.querySelectorAll('[data-search-item]'));
      var visibleItems = 0;

      items.forEach(function (item) {
        var itemCategory = normalize(item.getAttribute('data-content-category'));
        var itemSeverity = normalize(item.getAttribute('data-severity'));
        var itemText = normalize(item.getAttribute('data-search-text') || item.textContent);
        var matchesSearch = !searchTerm || itemText.indexOf(searchTerm) >= 0;
        var matchesCategory = category === 'all' || itemCategory === category;
        var matchesSeverity = itemCategory !== 'observations' || severity === 'all' || itemSeverity === severity;
        var visible = matchesSearch && matchesCategory && matchesSeverity;
        item.hidden = !visible;
        if (visible) {
          visibleItems += 1;
        }
      });

      Array.prototype.slice.call(document.querySelectorAll('[data-search-group]')).forEach(function (group) {
        var groupCategory = normalize(group.getAttribute('data-group-category'));
        var childItems = Array.prototype.slice.call(group.querySelectorAll('[data-search-item]'));
        var visibleChildren = childItems.filter(function (item) { return !item.hidden; }).length;
        var emptyState = group.querySelector('[data-empty-state]');
        var groupMatchesCategory = groupCategory === 'always' || category === 'all' || groupCategory === category;
        var shouldHideGroup = groupCategory !== 'always' && (!groupMatchesCategory || (childItems.length > 0 && visibleChildren === 0));
        group.hidden = shouldHideGroup;
        if (emptyState) {
          emptyState.hidden = visibleChildren !== 0 || !groupMatchesCategory;
        }
      });

      if (matchCount) {
        matchCount.textContent = String(visibleItems);
      }
    }

    [searchInput, severityFilter, categoryFilter].forEach(function (element) {
      if (!element) {
        return;
      }

      element.addEventListener('input', applyViewerFilters);
      element.addEventListener('change', applyViewerFilters);
    });

    applyViewerFilters();
  }());
</script>
""";
    }

    private static string JoinOrEmpty(IReadOnlyList<string> values)
    {
        return values.Count == 0
            ? string.Empty
            : string.Join(", ", values);
    }

    private static string EmptyToPlaceholder(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "N/A"
            : value;
    }

    private static string NormalizeToken(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }

    private static string EncodeAttribute(string value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
