using System.Text.Json;
using ATS.Application.Devices;
using ATS.Application.Flow;
using ATS.Application.Recipes;
using ATS.Application.Specs;
using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class TestRunner
{
    private readonly RecipeLoader _recipeLoader;
    private readonly RecipeValidator _recipeValidator;
    private readonly SpecLoader _specLoader;
    private readonly SpecValidator _specValidator;
    private readonly FlowEngine _flowEngine;
    private readonly SessionFactory _sessionFactory;
    private readonly SessionArtifactWriter _artifactWriter;

    public TestRunner()
        : this(
            new RecipeLoader(),
            new RecipeValidator(),
            new SpecLoader(),
            new SpecValidator(),
            new FlowEngine(new SpecEngine()),
            new SessionFactory(),
            new SessionArtifactWriter())
    {
    }

    public TestRunner(
        RecipeLoader recipeLoader,
        RecipeValidator recipeValidator,
        SpecLoader specLoader,
        SpecValidator specValidator,
        FlowEngine flowEngine,
        SessionFactory sessionFactory,
        SessionArtifactWriter artifactWriter)
    {
        _recipeLoader = recipeLoader;
        _recipeValidator = recipeValidator;
        _specLoader = specLoader;
        _specValidator = specValidator;
        _flowEngine = flowEngine;
        _sessionFactory = sessionFactory;
        _artifactWriter = artifactWriter;
    }

    public async Task<TestResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken)
    {
        var context = _sessionFactory.Create(
            request.CommandName,
            request.OutputDirectory,
            request.RecipePath,
            request.SpecPath,
            request.SelectedScriptName);

        var recipeName = string.Empty;
        var deviceName = "FakeDevice";
        var status = "Error";
        var scripts = new List<ScriptResult>();
        var errors = new List<string>();

        context.Log($"Session '{context.SessionId}' created for '{request.CommandName}'.");

        try
        {
            var recipe = _recipeLoader.Load(context.RecipePath);
            recipeName = recipe.Name;

            var specDocument = LoadSpecDocument(recipe, context.SpecPath);
            var specErrors = _specValidator.Validate(specDocument.Specs);
            var recipeErrors = _recipeValidator.Validate(recipe, specDocument.Specs, context.SelectedScriptName);

            foreach (var error in specErrors.Concat(recipeErrors))
            {
                errors.Add(error);
                context.LogError(error);
            }

            if (errors.Count == 0)
            {
                var specsByKey = specDocument.Specs.ToDictionary(
                    item => item.Key,
                    item => item,
                    StringComparer.OrdinalIgnoreCase);

                var flowResult = await _flowEngine.RunAsync(
                    recipe,
                    specsByKey,
                    new FakeDevice(),
                    context,
                    context.SelectedScriptName,
                    cancellationToken);

                deviceName = flowResult.DeviceName;
                status = flowResult.Status;
                scripts = flowResult.Scripts;
                errors.AddRange(flowResult.Errors);
            }
            else
            {
                status = "Invalid";
            }
        }
        catch (FileNotFoundException exception)
        {
            status = "Invalid";
            errors.Add(exception.Message);
            context.LogError(exception.Message);
        }
        catch (JsonException exception)
        {
            status = "Invalid";
            var message = $"Invalid JSON content: {exception.Message}";
            errors.Add(message);
            context.LogError(message);
        }
        catch (Exception exception)
        {
            status = "Error";
            errors.Add(exception.Message);
            context.LogError(exception.Message);
        }

        var result = new TestResult
        {
            SessionId = context.SessionId,
            CommandName = context.CommandName,
            RecipeName = recipeName,
            RecipePath = context.RecipePath,
            SpecPath = context.SpecPath,
            DeviceName = deviceName,
            Status = status,
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Scripts = scripts,
            Errors = errors
        };

        _artifactWriter.WriteTestResult(result, context);
        return result;
    }

    private SpecDocument LoadSpecDocument(RecipeDefinition recipe, string specPath)
    {
        if (!string.IsNullOrWhiteSpace(specPath))
        {
            return _specLoader.Load(specPath);
        }

        return new SpecDocument
        {
            Name = $"{recipe.Name} Inline Specs",
            Specs = recipe.Specs
        };
    }
}
