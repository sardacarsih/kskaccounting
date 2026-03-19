param(
    [string]$LogPath = ""
)

$candidatePatterns = @()
if (-not [string]::IsNullOrWhiteSpace($LogPath)) {
    $candidatePatterns += $LogPath
} else {
    $candidatePatterns += "Accounting/logs/log*.txt"
    $candidatePatterns += "logs/log*.txt"
    $candidatePatterns += ".\\log*.txt"
}

$files = @()
foreach ($pattern in $candidatePatterns) {
    $matches = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue | Sort-Object LastWriteTime
    if ($matches) {
        $files += $matches
    }
}
$files = $files | Sort-Object FullName -Unique
if (-not $files) {
    Write-Host "No log files found. Checked patterns:"
    $candidatePatterns | ForEach-Object { Write-Host " - $_" }
    exit 0
}

$pattern = 'PERF\s+(?<op>[^\s]+)\s+elapsed_ms=(?<ms>\d+)'
$rows = @()

foreach ($file in $files) {
    foreach ($line in Get-Content $file.FullName) {
        if ($line -match $pattern) {
            $rows += [PSCustomObject]@{
                Operation = $matches['op']
                ElapsedMs = [int]$matches['ms']
                File = $file.Name
            }
        }
    }
}

if (-not $rows) {
    Write-Host "No PERF entries found."
    exit 0
}

function Get-Percentile([int[]]$values, [double]$p) {
    if (-not $values -or $values.Count -eq 0) { return 0 }
    $sorted = $values | Sort-Object
    $rank = [Math]::Ceiling(($p / 100.0) * $sorted.Count)
    $index = [Math]::Max(0, [Math]::Min($sorted.Count - 1, $rank - 1))
    return $sorted[$index]
}

$summary = foreach ($g in ($rows | Group-Object Operation | Sort-Object Name)) {
    $vals = @($g.Group.ElapsedMs)
    [PSCustomObject]@{
        Operation = $g.Name
        Count = $vals.Count
        MinMs = ($vals | Measure-Object -Minimum).Minimum
        AvgMs = [Math]::Round(($vals | Measure-Object -Average).Average, 2)
        P95Ms = Get-Percentile -values $vals -p 95
        MaxMs = ($vals | Measure-Object -Maximum).Maximum
    }
}

$summary | Format-Table -AutoSize
