param(
    [string]$OutputPath = "CODEX_CONTEXT_SNAPSHOT.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

function Is-NoisePath {
    param(
        [string]$Path
    )

    $normalized = $Path.Replace("\", "/")
    $patterns = @(
        '^\.vs/',
        '^bin/',
        '^obj/',
        '^\.test-output/',
        '^\.codex-out/',
        '^\.ui-output/',
        '^logs/',
        '^json/',
        '^result(\.json|\.csv|_.*\.(json|csv))$',
        '^session.*\.(log|events\.jsonl)$',
        '(^|/)(bin|obj)/',
        '\.db-shm$',
        '\.db-wal$'
    )

    foreach ($pattern in $patterns) {
        if ($normalized -match $pattern) {
            return $true
        }
    }

    return $false
}

function Get-AreaName {
    param(
        [string]$Path
    )

    $normalized = $Path.Replace("\", "/")

    if ($normalized -like "src/ATS.Core/*") { return "src/ATS.Core" }
    if ($normalized -like "src/ATS.Application/*") { return "src/ATS.Application" }
    if ($normalized -like "src/ATS.Cli/*") { return "src/ATS.Cli" }
    if ($normalized -like "src/ATS.Ui/*") { return "src/ATS.Ui" }
    if ($normalized -like "tests/ATS.Tests/*") { return "tests/ATS.Tests" }
    if ($normalized -like "docs/*") { return "docs" }
    if ($normalized -like "samples/*") { return "samples" }
    if ($normalized -like "tools/*") { return "tools" }
    if ($normalized -notmatch "/") { return "repo-root" }
    return "other"
}

function Get-StatusEntries {
    $entries = @()
    $statusLines = git status --short --untracked-files=all

    foreach ($line in $statusLines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $status = if ($line.Length -ge 2) { $line.Substring(0, 2).Trim() } else { "?" }
        $rawPath = if ($line.Length -ge 4) { $line.Substring(3).Trim() } else { $line.Trim() }

        $path = $rawPath
        if ($rawPath -match ' -> ') {
            $path = ($rawPath -split ' -> ')[-1].Trim()
        }

        if (Is-NoisePath -Path $path) {
            continue
        }

        $entries += [pscustomobject]@{
            Status = if ([string]::IsNullOrWhiteSpace($status)) { "?" } else { $status }
            Path   = $path
            Area   = Get-AreaName -Path $path
        }
    }

    return $entries
}

$generatedAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz"
$branch = (git rev-parse --abbrev-ref HEAD).Trim()
$head = (git rev-parse --short HEAD).Trim()
$projects = (dotnet sln AutoTestSystem.Next.sln list | Select-Object -Skip 2) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
$changelogHeadings = Select-String -Path "CHANGELOG.md" -Pattern '^## \[' | Select-Object -First 5 | ForEach-Object { $_.Line.Trim() }
$entries = Get-StatusEntries
$groupedEntries = $entries | Group-Object Area | Sort-Object Name
$md = [char]96

$builder = New-Object System.Text.StringBuilder

[void]$builder.AppendLine("# CODEX_CONTEXT_SNAPSHOT")
[void]$builder.AppendLine()
[void]$builder.AppendLine("Generated: $generatedAt")
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Repo State")
[void]$builder.AppendLine()
[void]$builder.AppendLine("* Branch: ${md}$branch${md}")
[void]$builder.AppendLine("* HEAD: ${md}$head${md}")
[void]$builder.AppendLine("* Working tree entries after noise filtering: $($entries.Count)")
[void]$builder.AppendLine("* Noise filters applied: ${md}.vs${md}, build outputs, test outputs, generated session/result artifacts, local UI/Codex output folders")
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Read First")
[void]$builder.AppendLine()
[void]$builder.AppendLine("1. ${md}AGENTS.md${md}")
[void]$builder.AppendLine("2. ${md}CODEX_PLAN.md${md}")
[void]$builder.AppendLine("3. ${md}CODEX_CONTEXT.md${md}")
[void]$builder.AppendLine("4. ${md}CODEX_CONTEXT_SNAPSHOT.md${md}")
[void]$builder.AppendLine("5. ${md}CHANGELOG.md${md}")
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Solution Projects")
[void]$builder.AppendLine()
foreach ($project in $projects) {
    [void]$builder.AppendLine("* ${md}$($project.Trim())${md}")
}

[void]$builder.AppendLine()
[void]$builder.AppendLine("## Latest Changelog Entries")
[void]$builder.AppendLine()
foreach ($heading in $changelogHeadings) {
    [void]$builder.AppendLine("* $heading")
}

[void]$builder.AppendLine()
[void]$builder.AppendLine("## Filtered Working Tree Summary")
[void]$builder.AppendLine()

if ($entries.Count -eq 0) {
    [void]$builder.AppendLine("* No filtered changes detected.")
}
else {
    foreach ($group in $groupedEntries) {
        [void]$builder.AppendLine("### $($group.Name) ($($group.Count))")
        [void]$builder.AppendLine()

        foreach ($entry in ($group.Group | Sort-Object Path | Select-Object -First 12)) {
            [void]$builder.AppendLine("* ${md}[$($entry.Status)] $($entry.Path)${md}")
        }

        if ($group.Count -gt 12) {
            [void]$builder.AppendLine("* ${md}... +$($group.Count - 12) more${md}")
        }

        [void]$builder.AppendLine()
    }
}

[void]$builder.AppendLine("## Notes")
[void]$builder.AppendLine()
[void]$builder.AppendLine("* This snapshot is for fast review only. Open source files only after the snapshot shows the area that matters.")
[void]$builder.AppendLine("* If this file is stale, regenerate it with ${md}powershell -ExecutionPolicy Bypass -File tools/update-codex-context.ps1${md}.")

$outputFile = Join-Path $repoRoot $OutputPath
[System.IO.File]::WriteAllText($outputFile, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))
