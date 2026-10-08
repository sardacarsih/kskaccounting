<#
.SYNOPSIS
  Membuat paket update Accounting (ZIP) + manifest latest.json untuk di-upload ke server HTTPS.

.EXAMPLE
  .\tools\publish-update.ps1 -BaseUrl https://update.kskgroup.web.id/accounting -NotesFile .\catatan.txt
  .\tools\publish-update.ps1 -BaseUrl https://update.kskgroup.web.id/accounting -Notes "Perbaikan slip gaji" -Mandatory
  .\tools\publish-update.ps1 -NotesFile .\catatan.txt -UploadTarget dharyadi@ssh.kskgroup.web.id

.NOTES
  Naikkan <Version>, <AssemblyVersion>, dan <FileVersion> di Accounting\Accounting.csproj sebelum menjalankan script ini.
  Upload kedua file di folder output (ZIP dulu, latest.json terakhir) ke URL -BaseUrl,
  atau pakai -UploadTarget user@server untuk upload otomatis via SSH (lihat deploy/update-server/README.md).
#>
param(
    [string]$BaseUrl = "https://update.kskgroup.web.id/accounting",
    [string]$OutputDir,
    [string]$Runtime = "win-x64",
    [string]$Notes,
    [string]$NotesFile,
    [string]$MinimumVersion,
    [switch]$Mandatory,
    # user@host server update; kosong = tidak upload.
    [string]$UploadTarget,
    [string]$RemoteDir = "/srv/accounting-update/accounting",
    [int]$SshPort = 22
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "publish\update"
}

$baseUri = [Uri]$BaseUrl.TrimEnd('/')
if ($baseUri.Scheme -ne "https" -and -not $baseUri.IsLoopback) {
    throw "BaseUrl harus https (http hanya untuk localhost). Aplikasi akan menolak paket non-https."
}

$csproj = Join-Path $repoRoot "Accounting\Accounting.csproj"
[xml]$project = Get-Content -Raw $csproj
$version = $project.Project.PropertyGroup |
    ForEach-Object { $_.Version } |
    Where-Object { $_ } |
    Select-Object -First 1
if (-not $version) {
    throw "<Version> tidak ditemukan di Accounting\Accounting.csproj."
}
[void][Version]::Parse($version)

if ($MinimumVersion) {
    [void][Version]::Parse($MinimumVersion)
}

if ($NotesFile) {
    $Notes = Get-Content -Raw -Encoding UTF8 $NotesFile
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$publishDir = Join-Path $OutputDir "publish-$version"
if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

Write-Host "Publish Accounting $version ($Runtime)..."
dotnet publish $csproj -c Release -r $Runtime --self-contained false -p:PublishSingleFile=false -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish gagal."
}

# Accounting.Updater runs this after installing the files (--mode up, lalu --mode verify), so each
# site's database is migrated automatically. Migrasi SQL ditanam saat build, jadi selalu dipublish ulang.
Write-Host "Publish GLMigrator (single-file)..."
$migratorProject = Join-Path $repoRoot "Accounting\Utilities\Sql\GLMigrator\src\GLMigrator.Cli.csproj"
$migratorDir = Join-Path $OutputDir "migrator-$version"
if (Test-Path $migratorDir) {
    Remove-Item -Recurse -Force $migratorDir
}
dotnet publish $migratorProject -c Release -r $Runtime --self-contained false `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o $migratorDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish GLMigrator gagal."
}
Copy-Item (Join-Path $migratorDir "GLMigrator.exe") $publishDir -Force
Remove-Item -Recurse -Force $migratorDir

foreach ($required in @("Accounting.exe", "Accounting.Updater.exe", "Accounting.Updater.dll", "Accounting.Updater.runtimeconfig.json", "GLMigrator.exe")) {
    if (-not (Test-Path (Join-Path $publishDir $required))) {
        throw "$required tidak ada di hasil publish."
    }
}

# File per-site tidak boleh ikut paket (updater juga melewatinya).
foreach ($siteSpecific in @("Utilities\config.json", "config.json", "logs")) {
    $path = Join-Path $publishDir $siteSpecific
    if (Test-Path $path) {
        Remove-Item -Recurse -Force $path
    }
}
Get-ChildItem -Path $publishDir -Filter *.pdb -Recurse | Remove-Item -Force

$zipName = "Accounting-$version-$Runtime.zip"
$zipPath = Join-Path $OutputDir $zipName
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)
Remove-Item -Recurse -Force $publishDir

$zipInfo = Get-Item $zipPath
$sha256 = (Get-FileHash -Algorithm SHA256 $zipPath).Hash.ToLowerInvariant()

$manifest = [ordered]@{
    version     = $version
    mandatory   = [bool]$Mandatory
    packageUrl  = "$($baseUri.AbsoluteUri.TrimEnd('/'))/$zipName"
    sha256      = $sha256
    size        = $zipInfo.Length
    releaseDate = (Get-Date -Format "yyyy-MM-dd")
    notes       = if ($Notes) { $Notes.Trim() } else { "" }
}
if ($MinimumVersion) {
    $manifest.minimumVersion = $MinimumVersion
}

$manifestPath = Join-Path $OutputDir "latest.json"
$json = $manifest | ConvertTo-Json -Depth 3
[System.IO.File]::WriteAllText($manifestPath, $json, (New-Object System.Text.UTF8Encoding $false))

Write-Host ""
Write-Host "Paket   : $zipPath ($([Math]::Round($zipInfo.Length / 1MB, 1)) MB)"
Write-Host "SHA-256 : $sha256"
Write-Host "Manifest: $manifestPath"
Write-Host ""

if (-not $UploadTarget) {
    Write-Host "Upload $zipName lebih dulu, lalu latest.json, ke $($baseUri.AbsoluteUri)"
    return
}

# Upload as *.tmp then rename on the server, so clients never see a half-written file.
# The package goes first: latest.json must not point at a zip that is not there yet.
function Publish-Remote([string]$LocalPath, [string]$RemoteName) {
    $remoteTmp = "$RemoteDir/$RemoteName.tmp"
    Write-Host "Upload $RemoteName -> ${UploadTarget}:$RemoteDir"
    scp -P $SshPort -q $LocalPath "${UploadTarget}:$remoteTmp"
    if ($LASTEXITCODE -ne 0) { throw "scp $RemoteName gagal." }
    ssh -p $SshPort $UploadTarget "chmod 644 '$remoteTmp' && mv -f '$remoteTmp' '$RemoteDir/$RemoteName'"
    if ($LASTEXITCODE -ne 0) { throw "Gagal memindahkan $RemoteName di server." }
}

Publish-Remote $zipPath $zipName
$remoteHash = ((ssh -p $SshPort $UploadTarget "sha256sum '$RemoteDir/$zipName'") -split '\s+')[0]
if ($remoteHash -ne $sha256) {
    throw "SHA-256 paket di server ($remoteHash) tidak sama dengan lokal. latest.json TIDAK diupload."
}
Publish-Remote $manifestPath "latest.json"

Write-Host ""
Write-Host "Rilis $version terpasang. Cek: $($baseUri.AbsoluteUri.TrimEnd('/'))/latest.json"
