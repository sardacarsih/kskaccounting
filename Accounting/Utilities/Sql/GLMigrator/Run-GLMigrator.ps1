[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Connection,

    [ValidateSet("Up", "Down", "Status", "Verify")]
    [string]$Mode = "Up",

    [ValidateRange(1, 1000)]
    [int]$Steps = 1,

    [string]$ManifestPath = "",

    [string]$BootstrapScriptPath = "",

    [string]$LogDir = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $PSScriptRoot "migrations.manifest.json"
}

if ([string]::IsNullOrWhiteSpace($BootstrapScriptPath)) {
    $BootstrapScriptPath = Join-Path $PSScriptRoot "000_bootstrap_gl_migration_history.sql"
}

if ([string]::IsNullOrWhiteSpace($LogDir)) {
    $LogDir = Join-Path $PSScriptRoot "logs"
}

if (-not (Get-Command sqlplus -ErrorAction SilentlyContinue)) {
    throw "sqlplus command not found. Install Oracle Client / SQL*Plus first."
}

New-Item -Path $LogDir -ItemType Directory -Force | Out-Null

function Resolve-LocalPath {
    param([Parameter(Mandatory = $true)][string]$PathValue)

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    $candidate = Join-Path $PSScriptRoot $PathValue
    return [System.IO.Path]::GetFullPath($candidate)
}

function New-TempSqlScript {
    param([Parameter(Mandatory = $true)][string]$Content)
    $tmpFile = [System.IO.Path]::GetTempFileName() + ".sql"
    Set-Content -Path $tmpFile -Value $Content -Encoding ASCII
    return $tmpFile
}

function Invoke-SqlPlusFile {
    param(
        [Parameter(Mandatory = $true)][string]$SqlFilePath,
        [Parameter(Mandatory = $true)][string]$LogPrefix
    )

    $resolved = Resolve-LocalPath -PathValue $SqlFilePath
    if (-not (Test-Path -LiteralPath $resolved)) {
        throw "SQL file not found: $resolved"
    }

    $resolvedForSqlPlus = ($resolved -replace "\\", "/")
    $wrapperContent = @"
WHENEVER OSERROR EXIT FAILURE ROLLBACK
WHENEVER SQLERROR EXIT FAILURE ROLLBACK
SET SERVEROUTPUT ON
SET DEFINE ON
@$resolvedForSqlPlus
EXIT
"@

    $wrapperPath = New-TempSqlScript -Content $wrapperContent
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $logPath = Join-Path $LogDir "$($LogPrefix)_$timestamp.log"

    try {
        $output = & sqlplus -s $Connection "@$wrapperPath" 2>&1
        $exitCode = $LASTEXITCODE
        $outputText = ($output | Out-String)
        Set-Content -Path $logPath -Value $outputText -Encoding ASCII

        if ($exitCode -ne 0) {
            throw "SQL execution failed. See log: $logPath`n$outputText"
        }

        return [PSCustomObject]@{
            LogPath = $logPath
            Output  = $outputText
        }
    }
    finally {
        Remove-Item -LiteralPath $wrapperPath -Force -ErrorAction SilentlyContinue
    }
}

function Invoke-SqlPlusInline {
    param(
        [Parameter(Mandatory = $true)][string]$Sql,
        [Parameter(Mandatory = $true)][string]$LogPrefix
    )

    $tmpPath = New-TempSqlScript -Content $Sql
    try {
        return Invoke-SqlPlusFile -SqlFilePath $tmpPath -LogPrefix $LogPrefix
    }
    finally {
        Remove-Item -LiteralPath $tmpPath -Force -ErrorAction SilentlyContinue
    }
}

function Escape-SqlLiteral {
    param([AllowNull()][string]$Text)
    if ($null -eq $Text) {
        return "NULL"
    }

    $escaped = $Text -replace "'", "''"
    return "'$escaped'"
}

function Ensure-HistoryTable {
    Write-Host "[INFO] Ensuring GL_MIGRATION_HISTORY exists..."
    Invoke-SqlPlusFile -SqlFilePath $BootstrapScriptPath -LogPrefix "bootstrap" | Out-Null
}

function Get-AppliedMigrations {
    $statusSql = @"
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SET LINESIZE 32767
SET VERIFY OFF
SET ECHO OFF
SET TRIMSPOOL ON
SELECT MIGRATION_ID || '|' || CHECKSUM_SHA256 || '|' || TO_CHAR(APPLIED_AT, 'YYYY-MM-DD HH24:MI:SS')
  FROM GL_MIGRATION_HISTORY
 ORDER BY APPLIED_AT;
EXIT
"@

    $result = Invoke-SqlPlusInline -Sql $statusSql -LogPrefix "status_query"
    $lines = $result.Output -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -and $_.Contains("|") }

    $dict = @{}
    foreach ($line in $lines) {
        $parts = $line.Split("|")
        if ($parts.Length -ge 3) {
            $dict[$parts[0]] = [PSCustomObject]@{
                Checksum = $parts[1]
                AppliedAt = $parts[2]
            }
        }
    }

    return $dict
}

function Register-Migration {
    param(
        [Parameter(Mandatory = $true)]$Migration,
        [Parameter(Mandatory = $true)][string]$Checksum,
        [Parameter(Mandatory = $true)][long]$ExecutionMs
    )

    $idValue = Escape-SqlLiteral -Text $Migration.id
    $checksumValue = Escape-SqlLiteral -Text $Checksum
    $descriptionValue = Escape-SqlLiteral -Text $Migration.description
    $scriptPathValue = Escape-SqlLiteral -Text $Migration.script

    $insertSql = @"
INSERT INTO GL_MIGRATION_HISTORY
    (MIGRATION_ID, CHECKSUM_SHA256, DESCRIPTION, SCRIPT_PATH, STATUS, EXECUTION_MS)
VALUES
    ($idValue, $checksumValue, $descriptionValue, $scriptPathValue, 'SUCCESS', $ExecutionMs);
COMMIT;
EXIT
"@

    Invoke-SqlPlusInline -Sql $insertSql -LogPrefix "register_$($Migration.id)" | Out-Null
}

function Remove-MigrationHistory {
    param([Parameter(Mandatory = $true)][string]$MigrationId)
    $idValue = Escape-SqlLiteral -Text $MigrationId
    $deleteSql = @"
DELETE FROM GL_MIGRATION_HISTORY WHERE MIGRATION_ID = $idValue;
COMMIT;
EXIT
"@
    Invoke-SqlPlusInline -Sql $deleteSql -LogPrefix "unregister_$MigrationId" | Out-Null
}

$manifestResolved = Resolve-LocalPath -PathValue $ManifestPath
if (-not (Test-Path -LiteralPath $manifestResolved)) {
    throw "Manifest not found: $manifestResolved"
}

$manifest = Get-Content -Path $manifestResolved -Raw | ConvertFrom-Json
$migrations = @($manifest.migrations | Sort-Object order)

if ($Mode -in @("Up", "Down", "Status")) {
    Ensure-HistoryTable
}

switch ($Mode) {
    "Up" {
        $applied = Get-AppliedMigrations
        foreach ($migration in $migrations) {
            $scriptPath = Resolve-LocalPath -PathValue $migration.script
            $checksum = (Get-FileHash -Path $scriptPath -Algorithm SHA256).Hash.ToLowerInvariant()

            if ($applied.ContainsKey($migration.id)) {
                if ($applied[$migration.id].Checksum -ne $checksum) {
                    throw "Checksum mismatch for migration '$($migration.id)'. Applied=$($applied[$migration.id].Checksum), Current=$checksum"
                }

                Write-Host "[SKIP] $($migration.id) already applied at $($applied[$migration.id].AppliedAt)"
                continue
            }

            Write-Host "[APPLY] $($migration.id) -> $($migration.script)"
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            $null = Invoke-SqlPlusFile -SqlFilePath $migration.script -LogPrefix "up_$($migration.id)"
            $sw.Stop()

            Register-Migration -Migration $migration -Checksum $checksum -ExecutionMs $sw.ElapsedMilliseconds
            Write-Host "[DONE]  $($migration.id) ($($sw.ElapsedMilliseconds)ms)"
        }

        Write-Host "[OK] Migration Up completed."
    }
    "Down" {
        $applied = Get-AppliedMigrations
        $appliedRollbackable = @(
            $migrations |
                Where-Object { $applied.ContainsKey($_.id) -and $_.rollbackScript } |
                Sort-Object order -Descending
        )

        if ($appliedRollbackable.Count -eq 0) {
            Write-Host "[INFO] No applied rollbackable migrations."
            break
        }

        $toRollback = $appliedRollbackable | Select-Object -First $Steps
        foreach ($migration in $toRollback) {
            Write-Host "[ROLLBACK] $($migration.id) -> $($migration.rollbackScript)"
            $null = Invoke-SqlPlusFile -SqlFilePath $migration.rollbackScript -LogPrefix "down_$($migration.id)"
            Remove-MigrationHistory -MigrationId $migration.id
            Write-Host "[DONE]     $($migration.id)"
        }

        Write-Host "[OK] Migration Down completed."
    }
    "Status" {
        $applied = Get-AppliedMigrations
        foreach ($migration in $migrations) {
            if ($applied.ContainsKey($migration.id)) {
                Write-Host ("[APPLIED] {0} at {1}" -f $migration.id, $applied[$migration.id].AppliedAt)
            }
            else {
                Write-Host ("[PENDING] {0}" -f $migration.id)
            }
        }
    }
    "Verify" {
        foreach ($migration in $migrations) {
            if (-not $migration.checkScript) {
                continue
            }

            Write-Host "[VERIFY] $($migration.id) -> $($migration.checkScript)"
            $null = Invoke-SqlPlusFile -SqlFilePath $migration.checkScript -LogPrefix "verify_$($migration.id)"
        }

        Write-Host "[OK] Verification scripts completed."
    }
}
