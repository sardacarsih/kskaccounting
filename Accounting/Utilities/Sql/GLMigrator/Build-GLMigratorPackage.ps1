[CmdletBinding()]
param(
    [string]$OutputDir = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $PSScriptRoot "dist"
}

$manifestPath = Join-Path $PSScriptRoot "migrations.manifest.json"
if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Manifest not found: $manifestPath"
}

$manifest = Get-Content -Path $manifestPath -Raw | ConvertFrom-Json
$packageName = ($manifest.packageName -replace "[^A-Za-z0-9_-]", "")
if ([string]::IsNullOrWhiteSpace($packageName)) {
    $packageName = "GLMigrator"
}

$version = $manifest.version
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$bundleName = "{0}_{1}_{2}" -f $packageName, $version, $stamp

$stagingRoot = Join-Path $OutputDir $bundleName
$stagingMigrDir = Join-Path $stagingRoot "migrations"

New-Item -Path $stagingMigrDir -ItemType Directory -Force | Out-Null

Copy-Item (Join-Path $PSScriptRoot "Run-GLMigrator.ps1") $stagingRoot -Force
Copy-Item (Join-Path $PSScriptRoot "migrations.manifest.json") $stagingRoot -Force
Copy-Item (Join-Path $PSScriptRoot "000_bootstrap_gl_migration_history.sql") $stagingRoot -Force
Copy-Item (Join-Path $PSScriptRoot "README.md") $stagingRoot -Force

foreach ($migration in $manifest.migrations) {
    $source = Join-Path $PSScriptRoot $migration.script
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Migration script not found: $source"
    }

    $target = Join-Path $stagingRoot $migration.script
    $targetDir = Split-Path -Path $target -Parent
    New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
    Copy-Item $source $target -Force

    if ($migration.rollbackScript) {
        $rollbackSource = Join-Path $PSScriptRoot $migration.rollbackScript
        if (-not (Test-Path -LiteralPath $rollbackSource)) {
            throw "Rollback script not found: $rollbackSource"
        }

        $rollbackTarget = Join-Path $stagingRoot $migration.rollbackScript
        $rollbackTargetDir = Split-Path -Path $rollbackTarget -Parent
        New-Item -Path $rollbackTargetDir -ItemType Directory -Force | Out-Null
        Copy-Item $rollbackSource $rollbackTarget -Force
    }

    if ($migration.checkScript) {
        $checkSource = Join-Path $PSScriptRoot $migration.checkScript
        if (-not (Test-Path -LiteralPath $checkSource)) {
            throw "Check script not found: $checkSource"
        }

        $checkTarget = Join-Path $stagingRoot $migration.checkScript
        $checkTargetDir = Split-Path -Path $checkTarget -Parent
        New-Item -Path $checkTargetDir -ItemType Directory -Force | Out-Null
        Copy-Item $checkSource $checkTarget -Force
    }
}

$zipPath = Join-Path $OutputDir "$bundleName.zip"
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $stagingRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal -Force

Write-Host "Package folder : $stagingRoot"
Write-Host "Package zip    : $zipPath"
