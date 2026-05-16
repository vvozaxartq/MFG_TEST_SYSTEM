using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using ATS.Core.Models;
using ATS.Ui.Models;
using ATS.Ui.Services;

namespace ATS.Ui;

internal sealed class MainForm : Form
{
    private readonly ArtifactLoader _artifactLoader;
    private readonly CliCommandRunner _cliCommandRunner;
    private readonly List<CommandPreset> _commandPresets;

    private readonly ComboBox _commandPresetComboBox = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _commandArgumentsTextBox = new() { Dock = DockStyle.Fill };
    private readonly Button _runCommandButton = new() { Text = "Run CLI", Dock = DockStyle.Fill };
    private readonly TextBox _cliOutputTextBox = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true };
    private readonly TextBox _artifactPathTextBox = new() { Dock = DockStyle.Fill };
    private readonly Button _browseFileButton = new() { Text = "Browse File", Dock = DockStyle.Fill };
    private readonly Button _browseFolderButton = new() { Text = "Browse Folder", Dock = DockStyle.Fill };
    private readonly Button _loadArtifactsButton = new() { Text = "Load Artifacts", Dock = DockStyle.Fill };
    private readonly TextBox _summaryTextBox = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };
    private readonly TreeView _resultTreeView = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly TextBox _sessionLogTextBox = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true, Font = new Font("Consolas", 9F) };
    private readonly TextBox _structuredFilterTextBox = new() { Dock = DockStyle.Fill, PlaceholderText = "Filter by step, dut, key, message..." };
    private readonly ComboBox _structuredTypeComboBox = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _structuredLogGrid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false };
    private readonly TextBox _resultJsonPathTextBox = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly TextBox _structuredLogPathTextBox = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly TextBox _sessionLogPathTextBox = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly Button _openResultJsonButton = new() { Text = "Open result.json", Dock = DockStyle.Fill };
    private readonly Button _openStructuredLogButton = new() { Text = "Open structured log", Dock = DockStyle.Fill };
    private readonly Button _openSessionLogButton = new() { Text = "Open session.log", Dock = DockStyle.Fill };
    private readonly Label _statusLabel = new() { Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };

    private LoadedArtifacts? _loadedArtifacts;
    private IReadOnlyList<StructuredLogViewItem> _structuredLogItems = Array.Empty<StructuredLogViewItem>();

    public MainForm()
    {
        var repositoryRoot = WorkspaceLocator.FindRepositoryRoot();
        _artifactLoader = new ArtifactLoader();
        _cliCommandRunner = new CliCommandRunner(repositoryRoot);
        _commandPresets =
        [
            new CommandPreset("Test Simulate", "test simulate", "--recipe samples/recipes/demo.recipe.json --output .ui-output\\simulate"),
            new CommandPreset("Test Run", "test run", "--recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json --output .ui-output\\run"),
            new CommandPreset("Script Run", "script run", "--recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json --script ReadSerial --output .ui-output\\script"),
            new CommandPreset("Device Exec", "device exec", "--command PING --output .ui-output\\device"),
            new CommandPreset("Recipe Validate", "recipe validate", "--recipe samples/recipes/phase2.recipe.json --spec samples/specs/phase2.spec.json --output .ui-output\\recipe-validate"),
            new CommandPreset("Spec Validate", "spec validate", "--spec samples/specs/phase2.spec.json --output .ui-output\\spec-validate"),
            new CommandPreset("Custom", string.Empty, "test run --recipe samples/recipes/demo.recipe.json --output .ui-output\\custom")
        ];

        Text = "ATS Thin UI";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 800);
        Width = 1280;
        Height = 900;

        BuildLayout();
        WireEvents();
        InitializeState();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        root.Controls.Add(BuildCliRunnerPanel(), 0, 0);
        root.Controls.Add(BuildArtifactLoaderPanel(), 0, 1);
        root.Controls.Add(BuildMainTabs(), 0, 2);
        root.Controls.Add(_statusLabel, 0, 3);

        Controls.Add(root);
    }

    private Control BuildCliRunnerPanel()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Text = "CLI Runner"
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
        layout.Controls.Add(new Label { Text = "Command", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_commandPresetComboBox, 1, 0);
        layout.Controls.Add(_runCommandButton, 2, 0);
        layout.Controls.Add(new Label { Text = "Arguments", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(_commandArgumentsTextBox, 1, 1);
        layout.SetColumnSpan(_commandArgumentsTextBox, 2);
        layout.Controls.Add(new Label { Text = "CLI Output", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft }, 0, 2);
        layout.Controls.Add(_cliOutputTextBox, 1, 2);
        layout.SetColumnSpan(_cliOutputTextBox, 2);

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildArtifactLoaderPanel()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Text = "Artifact Loader"
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        layout.Controls.Add(new Label { Text = "Session Path", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_artifactPathTextBox, 1, 0);
        layout.Controls.Add(_browseFileButton, 2, 0);
        layout.Controls.Add(_browseFolderButton, 3, 0);
        layout.Controls.Add(_loadArtifactsButton, 3, 1);

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildMainTabs()
    {
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        tabControl.TabPages.Add(BuildSummaryTab());
        tabControl.TabPages.Add(BuildResultTab());
        tabControl.TabPages.Add(BuildStructuredLogTab());
        tabControl.TabPages.Add(BuildSessionLogTab());
        tabControl.TabPages.Add(BuildArtifactsTab());

        return tabControl;
    }

    private TabPage BuildSummaryTab()
    {
        var page = new TabPage("Session Summary");
        _summaryTextBox.Font = new Font("Consolas", 10F);
        page.Controls.Add(_summaryTextBox);
        return page;
    }

    private TabPage BuildResultTab()
    {
        var page = new TabPage("Result Viewer");
        page.Controls.Add(_resultTreeView);
        return page;
    }

    private TabPage BuildStructuredLogTab()
    {
        var page = new TabPage("Structured Log");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Controls.Add(_structuredFilterTextBox, 0, 0);
        layout.Controls.Add(_structuredTypeComboBox, 1, 0);
        layout.Controls.Add(_structuredLogGrid, 0, 1);
        layout.SetColumnSpan(_structuredLogGrid, 2);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildSessionLogTab()
    {
        var page = new TabPage("Session Log");
        page.Controls.Add(_sessionLogTextBox);
        return page;
    }

    private TabPage BuildArtifactsTab()
    {
        var page = new TabPage("Artifact Paths");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        layout.Controls.Add(new Label { Text = "result.json", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_resultJsonPathTextBox, 1, 0);
        layout.Controls.Add(_openResultJsonButton, 2, 0);
        layout.Controls.Add(new Label { Text = "session.events.jsonl", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(_structuredLogPathTextBox, 1, 1);
        layout.Controls.Add(_openStructuredLogButton, 2, 1);
        layout.Controls.Add(new Label { Text = "session.log", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        layout.Controls.Add(_sessionLogPathTextBox, 1, 2);
        layout.Controls.Add(_openSessionLogButton, 2, 2);

        page.Controls.Add(layout);
        return page;
    }

    private void WireEvents()
    {
        _commandPresetComboBox.SelectedIndexChanged += (_, _) => ApplySelectedPreset();
        _runCommandButton.Click += async (_, _) => await RunCliAsync();
        _browseFileButton.Click += (_, _) => BrowseArtifactFile();
        _browseFolderButton.Click += (_, _) => BrowseArtifactFolder();
        _loadArtifactsButton.Click += (_, _) => LoadArtifactsFromInput();
        _structuredFilterTextBox.TextChanged += (_, _) => ApplyStructuredLogFilter();
        _structuredTypeComboBox.SelectedIndexChanged += (_, _) => ApplyStructuredLogFilter();
        _openResultJsonButton.Click += (_, _) => OpenPath(_resultJsonPathTextBox.Text);
        _openStructuredLogButton.Click += (_, _) => OpenPath(_structuredLogPathTextBox.Text);
        _openSessionLogButton.Click += (_, _) => OpenPath(_sessionLogPathTextBox.Text);
    }

    private void InitializeState()
    {
        _commandPresetComboBox.DataSource = _commandPresets;
        _commandPresetComboBox.DisplayMember = nameof(CommandPreset.DisplayName);
        _structuredTypeComboBox.Items.Add("All Event Types");
        _structuredTypeComboBox.SelectedIndex = 0;
        ApplySelectedPreset();
        SetStatus("Ready.");
    }

    private void ApplySelectedPreset()
    {
        if (_commandPresetComboBox.SelectedItem is CommandPreset preset)
        {
            _commandArgumentsTextBox.Text = preset.DefaultArguments;
        }
    }

    private async Task RunCliAsync()
    {
        if (_commandPresetComboBox.SelectedItem is not CommandPreset preset)
        {
            return;
        }

        var cliArguments = BuildCliArguments(preset, _commandArgumentsTextBox.Text);
        if (string.IsNullOrWhiteSpace(cliArguments))
        {
            MessageBox.Show(this, "CLI arguments are required.", "ATS Thin UI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ToggleCommandInputs(enabled: false);
        _cliOutputTextBox.Clear();
        SetStatus("Running ATS CLI...");

        try
        {
            var progress = new Progress<string>(line => _cliOutputTextBox.AppendText(line + Environment.NewLine));
            var result = await _cliCommandRunner.RunAsync(cliArguments, progress, CancellationToken.None);
            SetStatus($"CLI completed with exit code {result.ExitCode}.");

            if (!string.IsNullOrWhiteSpace(result.ResultJsonPath))
            {
                _artifactPathTextBox.Text = result.ResultJsonPath;
                LoadArtifacts(result.ResultJsonPath);
            }
        }
        catch (Exception exception)
        {
            SetStatus("CLI run failed.");
            MessageBox.Show(this, exception.Message, "ATS Thin UI", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleCommandInputs(enabled: true);
        }
    }

    private static string BuildCliArguments(CommandPreset preset, string rawArguments)
    {
        if (string.Equals(preset.DisplayName, "Custom", StringComparison.OrdinalIgnoreCase))
        {
            return rawArguments.Trim();
        }

        return string.IsNullOrWhiteSpace(rawArguments)
            ? preset.BaseArguments
            : $"{preset.BaseArguments} {rawArguments.Trim()}";
    }

    private void BrowseArtifactFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Artifact Files|result.json;*.json;*.jsonl;*.log|All Files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _artifactPathTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseArtifactFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _artifactPathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void LoadArtifactsFromInput()
    {
        if (string.IsNullOrWhiteSpace(_artifactPathTextBox.Text))
        {
            MessageBox.Show(this, "Choose a session folder or artifact file first.", "ATS Thin UI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        LoadArtifacts(_artifactPathTextBox.Text);
    }

    private void LoadArtifacts(string artifactPath)
    {
        try
        {
            _loadedArtifacts = _artifactLoader.Load(artifactPath);
            RenderLoadedArtifacts(_loadedArtifacts);
            SetStatus($"Loaded artifacts from '{artifactPath}'.");
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "ATS Thin UI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Artifact load failed.");
        }
    }

    private void RenderLoadedArtifacts(LoadedArtifacts artifacts)
    {
        _summaryTextBox.Text = BuildSummaryText(artifacts);
        PopulateResultTree(artifacts);
        _sessionLogTextBox.Text = artifacts.SessionLogText;
        _resultJsonPathTextBox.Text = artifacts.ResultJsonPath;
        _structuredLogPathTextBox.Text = artifacts.StructuredLogPath;
        _sessionLogPathTextBox.Text = artifacts.SessionLogPath;

        _structuredLogItems = artifacts.StructuredEntries
            .Select(entry => new StructuredLogViewItem
            {
                Sequence = entry.Sequence,
                TimestampUtc = entry.TimestampUtc,
                ElapsedMs = entry.ElapsedMs,
                Level = entry.Level,
                EntryType = entry.EntryType.ToString(),
                ItemName = entry.ItemName,
                StepName = entry.StepName,
                DutId = entry.DutId,
                FullKey = entry.FullKey,
                Status = entry.Status,
                Message = entry.Message,
                DataPreview = BuildDataPreview(entry.Data)
            })
            .ToList();

        PopulateStructuredTypeFilter(_structuredLogItems);
        ApplyStructuredLogFilter();
    }

    private string BuildSummaryText(LoadedArtifacts artifacts)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Source: {artifacts.SourcePath}");
        builder.AppendLine($"result.json: {FormatPath(artifacts.ResultJsonPath)}");
        builder.AppendLine($"structured log: {FormatPath(artifacts.StructuredLogPath)}");
        builder.AppendLine($"session.log: {FormatPath(artifacts.SessionLogPath)}");
        builder.AppendLine();

        if (artifacts.TestResult is not null)
        {
            var result = artifacts.TestResult;
            builder.AppendLine("Type: Test Result");
            builder.AppendLine($"Session: {result.SessionId}");
            builder.AppendLine($"Command: {result.CommandName}");
            builder.AppendLine($"Recipe: {result.RecipeName}");
            builder.AppendLine($"Status: {result.Status}");
            builder.AppendLine($"SN: {FormatValue(result.RunInput.SerialNumber)}");
            builder.AppendLine($"Station: {FormatValue(result.RunInput.Station)}");
            builder.AppendLine($"Steps: {result.Steps.Count}");
            builder.AppendLine($"Errors: {result.Errors.Count}");
            return builder.ToString();
        }

        if (artifacts.DeviceCommandResult is not null)
        {
            var result = artifacts.DeviceCommandResult;
            builder.AppendLine("Type: Device Command Result");
            builder.AppendLine($"Session: {result.SessionId}");
            builder.AppendLine($"Device: {result.DeviceName}");
            builder.AppendLine($"Status: {result.Status}");
            builder.AppendLine($"Response: {result.Response}");
            return builder.ToString();
        }

        if (artifacts.ValidationResult is not null)
        {
            var result = artifacts.ValidationResult;
            builder.AppendLine("Type: Validation Result");
            builder.AppendLine($"Session: {result.SessionId}");
            builder.AppendLine($"Validation Type: {result.ValidationType}");
            builder.AppendLine($"Status: {result.Status}");
            builder.AppendLine($"Errors: {result.Errors.Count}");
            builder.AppendLine($"Warnings: {result.Warnings.Count}");
            return builder.ToString();
        }

        builder.AppendLine("Type: Unknown artifact");
        return builder.ToString();
    }

    private void PopulateResultTree(LoadedArtifacts artifacts)
    {
        _resultTreeView.BeginUpdate();
        _resultTreeView.Nodes.Clear();

        if (artifacts.TestResult is not null)
        {
            var result = artifacts.TestResult;
            var sessionNode = _resultTreeView.Nodes.Add($"Session [{result.Status}] {result.SessionId}");
            sessionNode.Nodes.Add($"Command: {result.CommandName}");
            sessionNode.Nodes.Add($"Recipe: {result.RecipeName}");

            foreach (var step in result.Steps)
            {
                var stepNode = sessionNode.Nodes.Add($"Step [{step.FinalStatus}] {step.StepName}");
                stepNode.Nodes.Add($"Command: {step.Command}");
                stepNode.Nodes.Add($"Prefix: {FormatValue(step.Prefix)}");
                stepNode.Nodes.Add($"RawPayload: {FormatValue(step.MeasurementSet.RawPayload)}");

                var measurementsNode = stepNode.Nodes.Add($"Measurements ({step.Measurements.Count})");
                foreach (var measurement in step.Measurements)
                {
                    measurementsNode.Nodes.Add($"{measurement.FullKey} = {measurement.Value} {measurement.Unit}".Trim());
                }

                var specNode = stepNode.Nodes.Add($"Spec Results ({step.SpecResults.Count})");
                foreach (var specResult in step.SpecResults)
                {
                    specNode.Nodes.Add($"[{specResult.PassFail}] {specResult.RuleName} | {specResult.TargetKey} | {specResult.Reason}");
                }

                if (!string.IsNullOrWhiteSpace(step.FailureMessage))
                {
                    stepNode.Nodes.Add($"Failure: {step.FailureMessage}");
                }
            }

            if (result.Errors.Count > 0)
            {
                var errorsNode = sessionNode.Nodes.Add($"Errors ({result.Errors.Count})");
                foreach (var error in result.Errors)
                {
                    errorsNode.Nodes.Add(error);
                }
            }

            sessionNode.Expand();
        }
        else if (artifacts.DeviceCommandResult is not null)
        {
            var result = artifacts.DeviceCommandResult;
            var root = _resultTreeView.Nodes.Add($"Device [{result.Status}] {result.DeviceName}");
            root.Nodes.Add($"Command: {result.Command}");
            root.Nodes.Add($"Response: {result.Response}");
            root.Nodes.Add($"Message: {result.Message}");
            root.Expand();
        }
        else if (artifacts.ValidationResult is not null)
        {
            var result = artifacts.ValidationResult;
            var root = _resultTreeView.Nodes.Add($"Validation [{result.Status}] {result.ValidationType}");
            var warningsNode = root.Nodes.Add($"Warnings ({result.Warnings.Count})");
            foreach (var warning in result.Warnings)
            {
                warningsNode.Nodes.Add(warning);
            }

            var errorsNode = root.Nodes.Add($"Errors ({result.Errors.Count})");
            foreach (var error in result.Errors)
            {
                errorsNode.Nodes.Add(error);
            }

            root.Expand();
        }

        _resultTreeView.EndUpdate();
    }

    private void PopulateStructuredTypeFilter(IReadOnlyList<StructuredLogViewItem> items)
    {
        var selected = _structuredTypeComboBox.SelectedItem?.ToString() ?? "All Event Types";
        var types = items
            .Select(item => item.EntryType)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _structuredTypeComboBox.BeginUpdate();
        _structuredTypeComboBox.Items.Clear();
        _structuredTypeComboBox.Items.Add("All Event Types");
        foreach (var type in types)
        {
            _structuredTypeComboBox.Items.Add(type);
        }

        _structuredTypeComboBox.SelectedItem = _structuredTypeComboBox.Items.Contains(selected)
            ? selected
            : "All Event Types";
        _structuredTypeComboBox.EndUpdate();
    }

    private void ApplyStructuredLogFilter()
    {
        var searchText = _structuredFilterTextBox.Text.Trim();
        var selectedType = _structuredTypeComboBox.SelectedItem?.ToString() ?? "All Event Types";

        var filtered = _structuredLogItems
            .Where(item => string.Equals(selectedType, "All Event Types", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(item.EntryType, selectedType, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(searchText) || MatchesStructuredSearch(item, searchText))
            .ToList();

        _structuredLogGrid.DataSource = new BindingList<StructuredLogViewItem>(filtered);
        _structuredLogGrid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private static bool MatchesStructuredSearch(StructuredLogViewItem item, string searchText)
    {
        return item.StepName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               item.ItemName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               item.DutId.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               item.FullKey.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               item.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               item.DataPreview.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDataPreview(IReadOnlyDictionary<string, object?> data)
    {
        return data.Count == 0
            ? string.Empty
            : string.Join("; ", data.Select(pair => $"{pair.Key}={pair.Value}"));
    }

    private void OpenPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(this, "Artifact file was not found.", "ATS Thin UI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private void ToggleCommandInputs(bool enabled)
    {
        _commandPresetComboBox.Enabled = enabled;
        _commandArgumentsTextBox.Enabled = enabled;
        _runCommandButton.Enabled = enabled;
    }

    private void SetStatus(string text)
    {
        _statusLabel.Text = text;
    }

    private static string FormatPath(string path) => string.IsNullOrWhiteSpace(path) ? "N/A" : path;

    private static string FormatValue(string value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value;
}

internal sealed record CommandPreset(string DisplayName, string BaseArguments, string DefaultArguments);

internal sealed class StructuredLogViewItem
{
    public long Sequence { get; init; }

    public DateTimeOffset TimestampUtc { get; init; }

    public long ElapsedMs { get; init; }

    public string Level { get; init; } = string.Empty;

    public string EntryType { get; init; } = string.Empty;

    public string ItemName { get; init; } = string.Empty;

    public string StepName { get; init; } = string.Empty;

    public string DutId { get; init; } = string.Empty;

    public string FullKey { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string DataPreview { get; init; } = string.Empty;
}
