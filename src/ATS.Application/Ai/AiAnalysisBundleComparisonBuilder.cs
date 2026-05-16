using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiAnalysisBundleComparisonBuilder
{
    public AiAnalysisBundleComparison Build(AiAnalysisBundle leftBundle, AiAnalysisBundle rightBundle)
    {
        ArgumentNullException.ThrowIfNull(leftBundle);
        ArgumentNullException.ThrowIfNull(rightBundle);

        var primaryCategory = BuildValueChange(
            "Primary Category",
            leftBundle.Analysis.PrimaryCategory,
            rightBundle.Analysis.PrimaryCategory);
        var primaryCause = BuildValueChange(
            "Primary Cause",
            leftBundle.Analysis.PrimaryCause,
            rightBundle.Analysis.PrimaryCause);
        var confidence = BuildValueChange(
            "Confidence",
            FormatConfidence(leftBundle.Analysis.Confidence),
            FormatConfidence(rightBundle.Analysis.Confidence));
        var summaryCountChanges = BuildSummaryCountChanges(leftBundle.Summary, rightBundle.Summary);
        var addedMatchedRules = BuildAddedValues(leftBundle.Analysis.MatchedRules, rightBundle.Analysis.MatchedRules);
        var removedMatchedRules = BuildRemovedValues(leftBundle.Analysis.MatchedRules, rightBundle.Analysis.MatchedRules);
        var addedFailedStepNames = BuildAddedValues(leftBundle.Summary.FailedStepNames, rightBundle.Summary.FailedStepNames);
        var removedFailedStepNames = BuildRemovedValues(leftBundle.Summary.FailedStepNames, rightBundle.Summary.FailedStepNames);
        var addedRecommendedActions = BuildAddedValues(leftBundle.Analysis.RecommendedActions, rightBundle.Analysis.RecommendedActions);
        var removedRecommendedActions = BuildRemovedValues(leftBundle.Analysis.RecommendedActions, rightBundle.Analysis.RecommendedActions);
        var addedEvidence = BuildAddedEvidence(leftBundle.Analysis.Evidence, rightBundle.Analysis.Evidence);
        var removedEvidence = BuildRemovedEvidence(leftBundle.Analysis.Evidence, rightBundle.Analysis.Evidence);

        return new AiAnalysisBundleComparison
        {
            LeftBundle = leftBundle,
            RightBundle = rightBundle,
            PrimaryCategory = primaryCategory,
            PrimaryCause = primaryCause,
            Confidence = confidence,
            SummaryCountChanges = summaryCountChanges,
            AddedMatchedRules = addedMatchedRules,
            RemovedMatchedRules = removedMatchedRules,
            AddedFailedStepNames = addedFailedStepNames,
            RemovedFailedStepNames = removedFailedStepNames,
            AddedRecommendedActions = addedRecommendedActions,
            RemovedRecommendedActions = removedRecommendedActions,
            AddedEvidence = addedEvidence,
            RemovedEvidence = removedEvidence,
            HasDifferences =
                primaryCategory.Changed ||
                primaryCause.Changed ||
                confidence.Changed ||
                summaryCountChanges.Count > 0 ||
                addedMatchedRules.Count > 0 ||
                removedMatchedRules.Count > 0 ||
                addedFailedStepNames.Count > 0 ||
                removedFailedStepNames.Count > 0 ||
                addedRecommendedActions.Count > 0 ||
                removedRecommendedActions.Count > 0 ||
                addedEvidence.Count > 0 ||
                removedEvidence.Count > 0
        };
    }

    private static AiComparisonValueChange BuildValueChange(string label, string leftValue, string rightValue)
    {
        var normalizedLeftValue = NormalizeOptional(leftValue);
        var normalizedRightValue = NormalizeOptional(rightValue);
        return new AiComparisonValueChange
        {
            Label = label,
            LeftValue = normalizedLeftValue,
            RightValue = normalizedRightValue,
            Changed = !string.Equals(normalizedLeftValue, normalizedRightValue, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static List<AiComparisonCountChange> BuildSummaryCountChanges(RunArtifactSummary leftSummary, RunArtifactSummary rightSummary)
    {
        var changes = new List<AiComparisonCountChange>();
        AddCountChange(changes, "Step Count", leftSummary.StepCount, rightSummary.StepCount);
        AddCountChange(changes, "Passed Step Count", leftSummary.PassedStepCount, rightSummary.PassedStepCount);
        AddCountChange(changes, "Failed Step Count", leftSummary.FailedStepCount, rightSummary.FailedStepCount);
        AddCountChange(changes, "Error Step Count", leftSummary.ErrorStepCount, rightSummary.ErrorStepCount);
        AddCountChange(changes, "Measurement Count", leftSummary.MeasurementCount, rightSummary.MeasurementCount);
        AddCountChange(changes, "Spec Count", leftSummary.SpecCount, rightSummary.SpecCount);
        AddCountChange(changes, "Failed Spec Count", leftSummary.FailedSpecCount, rightSummary.FailedSpecCount);
        AddCountChange(changes, "Error Count", leftSummary.ErrorCount, rightSummary.ErrorCount);
        AddCountChange(changes, "Variable Resolved Count", leftSummary.VariableResolvedCount, rightSummary.VariableResolvedCount);
        AddCountChange(changes, "Variable Resolution Failed Count", leftSummary.VariableResolutionFailedCount, rightSummary.VariableResolutionFailedCount);
        AddCountChange(changes, "Exception Count", leftSummary.ExceptionCount, rightSummary.ExceptionCount);
        AddCountChange(changes, "Warning Count", leftSummary.WarningCount, rightSummary.WarningCount);
        return changes;
    }

    private static void AddCountChange(List<AiComparisonCountChange> changes, string label, int leftValue, int rightValue)
    {
        if (leftValue == rightValue)
        {
            return;
        }

        changes.Add(new AiComparisonCountChange
        {
            Label = label,
            LeftValue = leftValue,
            RightValue = rightValue,
            Delta = rightValue - leftValue
        });
    }

    private static List<string> BuildAddedValues(IReadOnlyList<string> leftValues, IReadOnlyList<string> rightValues)
    {
        var leftSet = new HashSet<string>(
            leftValues.Where(static value => !string.IsNullOrWhiteSpace(value)),
            StringComparer.OrdinalIgnoreCase);

        return rightValues
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(value => !leftSet.Contains(value))
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> BuildRemovedValues(IReadOnlyList<string> leftValues, IReadOnlyList<string> rightValues)
    {
        var rightSet = new HashSet<string>(
            rightValues.Where(static value => !string.IsNullOrWhiteSpace(value)),
            StringComparer.OrdinalIgnoreCase);

        return leftValues
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(value => !rightSet.Contains(value))
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<AiEvidenceItem> BuildAddedEvidence(IReadOnlyList<AiEvidenceItem> leftEvidence, IReadOnlyList<AiEvidenceItem> rightEvidence)
    {
        var leftKeys = new HashSet<string>(leftEvidence.Select(BuildEvidenceKey), StringComparer.OrdinalIgnoreCase);

        return rightEvidence
            .Where(item => !leftKeys.Contains(BuildEvidenceKey(item)))
            .GroupBy(BuildEvidenceKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(BuildEvidenceKey, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<AiEvidenceItem> BuildRemovedEvidence(IReadOnlyList<AiEvidenceItem> leftEvidence, IReadOnlyList<AiEvidenceItem> rightEvidence)
    {
        var rightKeys = new HashSet<string>(rightEvidence.Select(BuildEvidenceKey), StringComparer.OrdinalIgnoreCase);

        return leftEvidence
            .Where(item => !rightKeys.Contains(BuildEvidenceKey(item)))
            .GroupBy(BuildEvidenceKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(BuildEvidenceKey, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildEvidenceKey(AiEvidenceItem item)
    {
        return string.Join(
            "|",
            NormalizeOptional(item.Type),
            NormalizeOptional(item.Message),
            NormalizeOptional(item.Source),
            NormalizeOptional(item.Value));
    }

    private static string FormatConfidence(double? confidence)
    {
        return confidence.HasValue
            ? confidence.Value.ToString("0.00")
            : "N/A";
    }

    private static string NormalizeOptional(string value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
