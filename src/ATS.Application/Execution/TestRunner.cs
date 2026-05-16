using System.Text.Json;
using ATS.Application.Devices;
using ATS.Application.Flow;
using ATS.Application.Recipes;
using ATS.Application.Specs;
using ATS.Core.Devices;
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
    private readonly IDeviceFactory _deviceFactory;

    public TestRunner()
        : this(
            new RecipeLoader(),
            new RecipeValidator(),
            new SpecLoader(),
            new SpecValidator(),
            new FlowEngine(new SpecEngine()),
            new SessionFactory(),
            new SessionArtifactWriter(),
            new FakeDeviceFactory())
    {
    }

    public TestRunner(
        RecipeLoader recipeLoader,
        RecipeValidator recipeValidator,
        SpecLoader specLoader,
        SpecValidator specValidator,
        FlowEngine flowEngine,
        SessionFactory sessionFactory,
        SessionArtifactWriter artifactWriter,
        IDeviceFactory deviceFactory)
    {
        _recipeLoader = recipeLoader;
        _recipeValidator = recipeValidator;
        _specLoader = specLoader;
        _specValidator = specValidator;
        _flowEngine = flowEngine;
        _sessionFactory = sessionFactory;
        _artifactWriter = artifactWriter;
        _deviceFactory = deviceFactory;
    }

    public async Task<TestResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken)
    {
        var context = _sessionFactory.Create(
            request.CommandName,
            request.OutputDirectory,
            request.ArtifactOptions,
            request.RunInput,
            request.RecipePath,
            request.SpecPath,
            request.SelectedScriptName);

        var recipeName = string.Empty;
        var deviceName = "FakeDevice";
        var status = "Error";
        var steps = new List<StepResult>();
        var scripts = new List<ScriptResult>();
        FlowNodeResult? flowResultTree = null;
        var errors = new List<string>();

        context.Log($"Session '{context.SessionId}' created for '{request.CommandName}'.", request.CommandName);

        try
        {
            var recipe = _recipeLoader.Load(context.RecipePath);
            recipeName = recipe.Name;

            var specDocument = LoadSpecDocument(recipe, context.SpecPath);
            var specErrors = _specValidator.Validate(specDocument);
            var recipeErrors = _recipeValidator.Validate(recipe, specDocument, context.SelectedScriptName);

            foreach (var error in specErrors.Concat(recipeErrors))
            {
                errors.Add(error);
                context.LogError(error, request.CommandName);
            }

            if (errors.Count == 0)
            {
                var flowResult = await _flowEngine.RunAsync(
                    recipe,
                    specDocument,
                    _deviceFactory.CreateDevice(),
                    context,
                    context.SelectedScriptName,
                    cancellationToken);

                deviceName = flowResult.DeviceName;
                status = flowResult.Status;
                steps = flowResult.Steps;
                scripts = flowResult.Scripts;
                flowResultTree = flowResult.FlowResultTree;
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
            context.LogError(exception.Message, request.CommandName);
        }
        catch (JsonException exception)
        {
            status = "Invalid";
            var message = $"Invalid JSON content: {exception.Message}";
            errors.Add(message);
            context.LogError(message, request.CommandName);
        }
        catch (Exception exception)
        {
            status = "Error";
            errors.Add(exception.Message);
            context.LogError(exception.Message, request.CommandName);
        }

        var completedAtUtc = DateTimeOffset.UtcNow;
        var result = new TestResult
        {
            SessionId = context.SessionId,
            CommandName = context.CommandName,
            RecipeName = recipeName,
            RecipePath = context.RecipePath,
            SpecPath = context.SpecPath,
            DeviceName = deviceName,
            Status = status,
            OutputDirectory = context.OutputDirectory,
            ResultJsonPath = context.ArtifactPaths.ResultJsonPath,
            ResultCsvPath = context.ArtifactPaths.ResultCsvPath,
            SessionLogPath = context.ArtifactPaths.SessionLogPath,
            StructuredLogPath = context.ArtifactPaths.StructuredLogPath,
            RunInput = context.RunInput,
            SessionInfo = SessionInfoBuilder.Build(context, recipeName, status, completedAtUtc),
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Steps = steps,
            Scripts = scripts,
            FlowResultTree = flowResultTree,
            Errors = errors
        };

        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.SessionCompleted,
            $"Session '{context.SessionId}' completed with status '{status}'.",
            request.CommandName,
            status: status,
            data: new Dictionary<string, object?>
            {
                ["recipeName"] = recipeName,
                ["resultJsonPath"] = context.ArtifactPaths.ResultJsonPath,
                ["resultCsvPath"] = context.ArtifactPaths.ResultCsvPath,
                ["sessionLogPath"] = context.ArtifactPaths.SessionLogPath,
                ["structuredLogPath"] = context.ArtifactPaths.StructuredLogPath
            });
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
            Rules = recipe.Rules,
            Specs = recipe.Specs
        };
    }
}
