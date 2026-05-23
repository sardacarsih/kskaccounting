[CmdletBinding()]
param(
    [string]$OutputDir = "",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$builderPath = Join-Path $PSScriptRoot "Build-GLMigratorExePackage.ps1"
if (-not (Test-Path -LiteralPath $builderPath)) {
    throw "EXE package builder not found: $builderPath"
}

Write-Host "Build-GLMigratorPackage.ps1 now delegates to EXE packaging only."

$invokeParams = @{
    OutputDir = $OutputDir
    Runtime = $Runtime
    SelfContained = $SelfContained
}

& $builderPath @invokeParams
