using System.Text.Json;
using ATS.Application.Execution;
using ATS.Core.Models;

namespace ATS.Application.Specs;

public sealed class SpecValidationService
{
    private readonly SpecLoader _specLoader;
    private readonly SpecValidator _specValidator;
    private readonly SessionFactory _sessionFactory;
    private readonly SessionArtifactWriter _artifactWriter;

    public SpecValidationService()
        : this(new SpecLoader(), new SpecValidator(), new SessionFactory(), new SessionArtifactWriter())
    {
    }

    public SpecValidationService(
        SpecLoader specLoader,
        SpecValidator specValidator,
        SessionFactory sessionFactory,
        SessionArtifactWriter artifactWriter)
    {
        _specLoader = specLoader;
        _specValidator = specValidator;
        _sessionFactory = sessionFactory;
        _artifactWriter = artifactWriter;
    }

    public Task<ValidationResult> ValidateAsync(
        string specPath,
        string outputDirectory,
        SessionArtifactOptions? artifactOptions,
        RunInputModel? runInput,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = _sessionFactory.Create(
            "spec validate",
            outputDirectory,
            artifactOptions,
            runInput,
            specPath: specPath);
        var errors = new List<string>();
        var status = "Passed";

        context.Log($"Session '{context.SessionId}' created for 'spec validate'.", "spec validate");

        try
        {
            var specDocument = _specLoader.Load(context.SpecPath);
            errors.AddRange(_specValidator.Validate(specDocument));

            foreach (var error in errors)
            {
                context.LogError(error, "spec validate");
            }

            if (errors.Count > 0)
            {
                status = "Failed";
            }
        }
        catch (FileNotFoundException exception)
        {
            status = "Failed";
            errors.Add(exception.Message);
            context.LogError(exception.Message, "spec validate");
        }
        catch (JsonException exception)
        {
            status = "Failed";
            var message = $"Invalid JSON content: {exception.Message}";
            errors.Add(message);
            context.LogError(message, "spec validate");
        }
        catch (Exception exception)
        {
            status = "Failed";
            errors.Add(exception.Message);
            context.LogError(exception.Message, "spec validate");
        }

        var completedAtUtc = DateTimeOffset.UtcNow;
        var result = new ValidationResult
        {
            SessionId = context.SessionId,
            CommandName = context.CommandName,
            ValidationType = "spec",
            TargetPath = context.SpecPath,
            RelatedPath = string.Empty,
            Status = status,
            OutputDirectory = context.OutputDirectory,
            ResultJsonPath = context.ArtifactPaths.ResultJsonPath,
            ResultCsvPath = context.ArtifactPaths.ResultCsvPath,
            SessionLogPath = context.ArtifactPaths.SessionLogPath,
            StructuredLogPath = context.ArtifactPaths.StructuredLogPath,
            RunInput = context.RunInput,
            SessionInfo = SessionInfoBuilder.Build(context, string.Empty, status, completedAtUtc),
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Errors = errors
        };

        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.SessionCompleted,
            $"Session '{context.SessionId}' completed with status '{status}'.",
            "spec validate",
            status: status,
            data: new Dictionary<string, object?>
            {
                ["validationType"] = "spec",
                ["targetPath"] = context.SpecPath,
                ["resultJsonPath"] = context.ArtifactPaths.ResultJsonPath,
                ["resultCsvPath"] = context.ArtifactPaths.ResultCsvPath,
                ["sessionLogPath"] = context.ArtifactPaths.SessionLogPath,
                ["structuredLogPath"] = context.ArtifactPaths.StructuredLogPath
            });
        _artifactWriter.WriteValidationResult(result, context);
        return Task.FromResult(result);
    }
}
