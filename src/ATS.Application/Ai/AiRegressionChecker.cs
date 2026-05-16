using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiRegressionChecker
{
    private const string CurrentSchemaVersion = "ats.ai-regression-check.v1";

    public AiRegressionCheckResult Check(AiAnalysisBundle baselineBundle, AiAnalysisBundle candidateBundle)
    {
        ArgumentNullException.ThrowIfNull(baselineBundle);
        ArgumentNullException.ThrowIfNull(candidateBundle);

        var findings = new List<AiRegressionFinding>();
        AddPrimaryCategoryFinding(findings, baselineBundle, candidateBundle);
        AddCountIncreaseFinding(
            findings,
            "FailedSpecCountIncreased",
            "Failed Spec Count Increased",
            "RunArtifactSummary.FailedSpecCount",
            baselineBundle.Summary.FailedSpecCount,
            candidateBundle.Summary.FailedSpecCount);
        AddCountIncreaseFinding(
            findings,
            "ExceptionCountIncreased",
            "Exception Count Increased",
            "RunArtifactSummary.ExceptionCount",
            baselineBundle.Summary.ExceptionCount,
            candidateBundle.Summary.ExceptionCount);
        AddVariableResolutionFinding(findings, baselineBundle, candidateBundle);
        AddNewFailedStepFinding(findings, baselineBundle, candidateBundle);
        AddMoreSevereRuleFinding(findings, baselineBundle, candidateBundle);

        return new AiRegressionCheckResult
        {
            SchemaVersion = CurrentSchemaVersion,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Status = findings.Count == 0
                ? AiRegressionStatus.NoRegression
                : AiRegressionStatus.RegressionDetected,
            Summary = findings.Count == 0
                ? "Candidate did not regress relative to baseline across the configured deterministic checks."
                : $"Detected {findings.Count} regression findings relative to the baseline bundle.",
            BaselineBundlePath = Path.GetFullPath(baselineBundle.ResultJsonPath),
            CandidateBundlePath = Path.GetFullPath(candidateBundle.ResultJsonPath),
            BaselinePrimaryCategory = baselineBundle.Analysis.PrimaryCategory,
            CandidatePrimaryCategory = candidateBundle.Analysis.PrimaryCategory,
            BaselinePrimaryCause = baselineBundle.Analysis.PrimaryCause,
            CandidatePrimaryCause = candidateBundle.Analysis.PrimaryCause,
            Findings = findings
        };
    }

    private static void AddPrimaryCategoryFinding(
        List<AiRegressionFinding> findings,
        AiAnalysisBundle baselineBundle,
        AiAnalysisBundle candidateBundle)
    {
        var baselineSeverity = GetCategorySeverity(baselineBundle.Analysis.PrimaryCategory);
        var candidateSeverity = GetCategorySeverity(candidateBundle.Analysis.PrimaryCategory);
        if (candidateSeverity <= baselineSeverity)
        {
            return;
        }

        findings.Add(new AiRegressionFinding
        {
            Code = "PrimaryCategoryWorsened",
            Title = "Primary Category Worsened",
            Message = $"Primary category worsened from {Fallback(baselineBundle.Analysis.PrimaryCategory)} to {Fallback(candidateBundle.Analysis.PrimaryCategory)}.",
            Source = "AiRunAnalysisResult.PrimaryCategory",
            BaselineValue = Fallback(baselineBundle.Analysis.PrimaryCategory),
            CandidateValue = Fallback(candidateBundle.Analysis.PrimaryCategory)
        });
    }

    private static void AddCountIncreaseFinding(
        List<AiRegressionFinding> findings,
        string code,
        string title,
        string source,
        int baselineValue,
        int candidateValue)
    {
        if (candidateValue <= baselineValue)
        {
            return;
        }

        findings.Add(new AiRegressionFinding
        {
            Code = code,
            Title = title,
            Message = $"{title} from {baselineValue} to {candidateValue}.",
            Source = source,
            BaselineValue = baselineValue.ToString(),
            CandidateValue = candidateValue.ToString()
        });
    }

    private static void AddVariableResolutionFinding(
        List<AiRegressionFinding> findings,
        AiAnalysisBundle baselineBundle,
        AiAnalysisBundle candidateBundle)
    {
        var baselineValue = baselineBundle.Summary.VariableResolutionFailedCount;
        var candidateValue = candidateBundle.Summary.VariableResolutionFailedCount;
        if (candidateValue <= baselineValue)
        {
            return;
        }

        var detail = string.IsNullOrWhiteSpace(candidateBundle.Summary.FirstFailureMessage)
            ? string.Empty
            : $" First candidate failure: {candidateBundle.Summary.FirstFailureMessage}";

        findings.Add(new AiRegressionFinding
        {
            Code = "VariableResolutionFailuresIncreased",
            Title = "Variable Resolution Failures Increased",
            Message = $"Variable resolution failures increased from {baselineValue} to {candidateValue}.{detail}".Trim(),
            Source = "RunArtifactSummary.VariableResolutionFailedCount",
            BaselineValue = baselineValue.ToString(),
            CandidateValue = candidateValue.ToString()
        });
    }

    private static void AddNewFailedStepFinding(
        List<AiRegressionFinding> findings,
        AiAnalysisBundle baselineBundle,
        AiAnalysisBundle candidateBundle)
    {
        var baselineSteps = new HashSet<string>(GetFailedStepNames(baselineBundle), StringComparer.OrdinalIgnoreCase);
        var candidateSteps = GetFailedStepNames(candidateBundle)
            .Where(stepName => !baselineSteps.Contains(stepName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(stepName => stepName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidateSteps.Count == 0)
        {
            return;
        }

        findings.Add(new AiRegressionFinding
        {
            Code = "NewFailedStepNamesDetected",
            Title = "New Failed Step Names Detected",
            Message = $"New failed step names appeared: {string.Join(", ", candidateSteps)}.",
            Source = "RunArtifactSummary.FailedStepNames",
            BaselineValue = string.Join(", ", GetFailedStepNames(baselineBundle)),
            CandidateValue = string.Join(", ", GetFailedStepNames(candidateBundle))
        });
    }

    private static void AddMoreSevereRuleFinding(
        List<AiRegressionFinding> findings,
        AiAnalysisBundle baselineBundle,
        AiAnalysisBundle candidateBundle)
    {
        var baselineMaxSeverity = baselineBundle.Analysis.MatchedRules.Count == 0
            ? 0
            : baselineBundle.Analysis.MatchedRules.Max(GetRuleSeverity);
        var baselineRules = new HashSet<string>(baselineBundle.Analysis.MatchedRules, StringComparer.OrdinalIgnoreCase);
        var moreSevereRules = candidateBundle.Analysis.MatchedRules
            .Where(ruleName => !baselineRules.Contains(ruleName))
            .Where(ruleName => GetRuleSeverity(ruleName) > baselineMaxSeverity)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(ruleName => ruleName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (moreSevereRules.Count == 0)
        {
            return;
        }

        findings.Add(new AiRegressionFinding
        {
            Code = "MoreSevereMatchedRulesAppeared",
            Title = "More Severe Matched Rules Appeared",
            Message = $"New more severe matched rules appeared: {string.Join(", ", moreSevereRules)}.",
            Source = "AiRunAnalysisResult.MatchedRules",
            BaselineValue = string.Join(", ", baselineBundle.Analysis.MatchedRules),
            CandidateValue = string.Join(", ", candidateBundle.Analysis.MatchedRules)
        });
    }

    private static IReadOnlyList<string> GetFailedStepNames(AiAnalysisBundle bundle)
    {
        return bundle.Summary.FailedStepNames
            .Concat(bundle.Summary.ErrorStepNames)
            .Where(stepName => !string.IsNullOrWhiteSpace(stepName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int GetCategorySeverity(string category)
    {
        return category.Trim().ToUpperInvariant() switch
        {
            "SUCCESS" => 100,
            "SPEC" => 200,
            "EXECUTION" => 300,
            "RUNTIME" => 400,
            "CONFIGURATION" => 500,
            _ => 250
        };
    }

    private static int GetRuleSeverity(string ruleName)
    {
        return ruleName.Trim().ToUpperInvariant() switch
        {
            "SUCCESSRULE" => 100,
            "SPECFAILURERULE" => 200,
            "STEPFAILURERULE" => 300,
            "UNHANDLEDEXCEPTIONRULE" => 400,
            "VARIABLERESOLUTIONFAILURERULE" => 500,
            _ => 250
        };
    }

    private static string Fallback(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "N/A"
            : value;
    }
}
