[CmdletBinding()]
param(
    [string]$OutputDir = "",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $PSScriptRoot "dist-exe"
}

$manifestPath = Join-Path $PSScriptRoot "migrations.manifest.json"
if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Manifest not found: $manifestPath"
}

$manifest = Get-Content -Path $manifestPath -Raw | ConvertFrom-Json
$packageName = (($manifest.packageName -replace "[^A-Za-z0-9_-]", "") + "Exe")
$version = $manifest.version
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$bundleName = "{0}_{1}_{2}" -f $packageName, $version, $stamp

$publishDir = Join-Path $PSScriptRoot ".publish\$bundleName"
New-Item -Path $publishDir -ItemType Directory -Force | Out-Null

$projectPath = Join-Path $PSScriptRoot "src/GLMigrator.Cli.csproj"
if ($SelfContained) {
    dotnet restore $projectPath `
      -r $Runtime `
      -p:RestoreIgnoreFailedSources=true `
      -p:NuGetAudit=false

    dotnet publish $projectPath `
      -c Release `
      -r $Runtime `
      --self-contained true `
      /p:PublishSingleFile=true `
      /p:PublishTrimmed=false `
      --no-restore `
      -o $publishDir
}
else {
    dotnet restore $projectPath `
      -p:RestoreIgnoreFailedSources=true `
      -p:NuGetAudit=false

    dotnet publish $projectPath `
      -c Release `
      /p:UseAppHost=true `
      --no-restore `
      -o $publishDir
}

$exePath = Join-Path $publishDir "GLMigrator.exe"
if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Published EXE not found: $exePath"
}

$stagingRoot = Join-Path $OutputDir $bundleName
New-Item -Path $stagingRoot -ItemType Directory -Force | Out-Null
New-Item -Path (Join-Path $stagingRoot "migrations") -ItemType Directory -Force | Out-Null

Get-ChildItem -Path $publishDir -File | ForEach-Object {
    Copy-Item $_.FullName (Join-Path $stagingRoot $_.Name) -Force
}
Copy-Item (Join-Path $PSScriptRoot "migrations.manifest.json") $stagingRoot -Force
Copy-Item (Join-Path $PSScriptRoot "000_bootstrap_gl_migration_history.sql") $stagingRoot -Force
Copy-Item (Join-Path $PSScriptRoot "README.md") $stagingRoot -Force

foreach ($migration in $manifest.migrations) {
    foreach ($key in @("script", "rollbackScript", "checkScript")) {
        if (-not $migration.$key) { continue }
        $src = Join-Path $PSScriptRoot $migration.$key
        if (-not (Test-Path -LiteralPath $src)) {
            throw "Migration file missing: $src"
        }
        $dst = Join-Path $stagingRoot $migration.$key
        $dstDir = Split-Path -Path $dst -Parent
        New-Item -Path $dstDir -ItemType Directory -Force | Out-Null
        Copy-Item $src $dst -Force
    }
}

$zipPath = Join-Path $OutputDir "$bundleName.zip"
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $stagingRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal -Force

Write-Host "EXE package folder : $stagingRoot"
Write-Host "EXE package zip    : $zipPath"
