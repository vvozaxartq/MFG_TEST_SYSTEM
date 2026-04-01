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
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = _sessionFactory.Create("spec validate", outputDirectory, specPath: specPath);
        var errors = new List<string>();
        var status = "Passed";

        context.Log($"Session '{context.SessionId}' created for 'spec validate'.");

        try
        {
            var specDocument = _specLoader.Load(context.SpecPath);
            errors.AddRange(_specValidator.Validate(specDocument));

            foreach (var error in errors)
            {
                context.LogError(error);
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
            context.LogError(exception.Message);
        }
        catch (JsonException exception)
        {
            status = "Failed";
            var message = $"Invalid JSON content: {exception.Message}";
            errors.Add(message);
            context.LogError(message);
        }
        catch (Exception exception)
        {
            status = "Failed";
            errors.Add(exception.Message);
            context.LogError(exception.Message);
        }

        var result = new ValidationResult
        {
            SessionId = context.SessionId,
            CommandName = context.CommandName,
            ValidationType = "spec",
            TargetPath = context.SpecPath,
            RelatedPath = string.Empty,
            Status = status,
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Errors = errors
        };

        _artifactWriter.WriteValidationResult(result, context);
        return Task.FromResult(result);
    }
}
