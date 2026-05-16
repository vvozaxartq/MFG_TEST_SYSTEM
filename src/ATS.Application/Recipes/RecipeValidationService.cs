using System.Text.Json;
using ATS.Application.Execution;
using ATS.Application.Specs;
using ATS.Core.Models;

namespace ATS.Application.Recipes;

public sealed class RecipeValidationService
{
    private readonly RecipeLoader _recipeLoader;
    private readonly RecipeValidator _recipeValidator;
    private readonly SpecLoader _specLoader;
    private readonly SpecValidator _specValidator;
    private readonly SessionFactory _sessionFactory;
    private readonly SessionArtifactWriter _artifactWriter;

    public RecipeValidationService()
        : this(
            new RecipeLoader(),
            new RecipeValidator(),
            new SpecLoader(),
            new SpecValidator(),
            new SessionFactory(),
            new SessionArtifactWriter())
    {
    }

    public RecipeValidationService(
        RecipeLoader recipeLoader,
        RecipeValidator recipeValidator,
        SpecLoader specLoader,
        SpecValidator specValidator,
        SessionFactory sessionFactory,
        SessionArtifactWriter artifactWriter)
    {
        _recipeLoader = recipeLoader;
        _recipeValidator = recipeValidator;
        _specLoader = specLoader;
        _specValidator = specValidator;
        _sessionFactory = sessionFactory;
        _artifactWriter = artifactWriter;
    }

    public Task<ValidationResult> ValidateAsync(
        string recipePath,
        string specPath,
        string outputDirectory,
        SessionArtifactOptions? artifactOptions,
        RunInputModel? runInput,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = _sessionFactory.Create(
            "recipe validate",
            outputDirectory,
            artifactOptions,
            runInput,
            recipePath,
            specPath);
        var errors = new List<string>();
        var warnings = new List<string>();
        var status = "Passed";

        context.Log($"Session '{context.SessionId}' created for 'recipe validate'.", "recipe validate");

        try
        {
            var recipe = _recipeLoader.Load(context.RecipePath);
            var specDocument = string.IsNullOrWhiteSpace(context.SpecPath)
                ? new SpecDocument
                {
                    Name = $"{recipe.Name} Inline Specs",
                    Rules = recipe.Rules,
                    Specs = recipe.Specs
                }
                : _specLoader.Load(context.SpecPath);

            errors.AddRange(_specValidator.Validate(specDocument));
            errors.AddRange(_recipeValidator.Validate(recipe, specDocument, string.Empty));
            warnings.AddRange(_recipeValidator.GetValidationWarnings(recipe));

            foreach (var error in errors)
            {
                context.LogError(error, "recipe validate");
            }

            foreach (var warning in warnings)
            {
                context.Log(warning, "recipe validate");
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
            context.LogError(exception.Message, "recipe validate");
        }
        catch (JsonException exception)
        {
            status = "Failed";
            var message = $"Invalid JSON content: {exception.Message}";
            errors.Add(message);
            context.LogError(message, "recipe validate");
        }
        catch (Exception exception)
        {
            status = "Failed";
            errors.Add(exception.Message);
            context.LogError(exception.Message, "recipe validate");
        }

        var completedAtUtc = DateTimeOffset.UtcNow;
        var result = new ValidationResult
        {
            SessionId = context.SessionId,
            CommandName = context.CommandName,
            ValidationType = "recipe",
            TargetPath = context.RecipePath,
            RelatedPath = context.SpecPath,
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
            Errors = errors,
            Warnings = warnings
        };

        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.SessionCompleted,
            $"Session '{context.SessionId}' completed with status '{status}'.",
            "recipe validate",
            status: status,
            data: new Dictionary<string, object?>
            {
                ["validationType"] = "recipe",
                ["targetPath"] = context.RecipePath,
                ["warningCount"] = warnings.Count,
                ["resultJsonPath"] = context.ArtifactPaths.ResultJsonPath,
                ["resultCsvPath"] = context.ArtifactPaths.ResultCsvPath,
                ["sessionLogPath"] = context.ArtifactPaths.SessionLogPath,
                ["structuredLogPath"] = context.ArtifactPaths.StructuredLogPath
            });
        _artifactWriter.WriteValidationResult(result, context);
        return Task.FromResult(result);
    }
}
