#Requires -Version 5.1
<#
.SYNOPSIS
  Reproducible release build for PCOptimizer.
  Produces: PCOptimizer.exe, PCOptimizer.Updater.exe, checksums, latest.json
#>
param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release",
    [string]$Channel = "stable",
    [switch]$SkipInstaller,
    [string]$SigningCertPath = "",
    [string]$SigningCertPassword = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path (Join-Path $Root "UltimateWindowsOptimizer.sln"))) {
    $Root = Split-Path -Parent $PSScriptRoot
}
Set-Location $Root

$Artifacts = Join-Path $Root "artifacts"
$PublishApp = Join-Path $Artifacts "publish\app"
$PublishUpdater = Join-Path $Artifacts "publish\updater"
$PackageDir = Join-Path $Artifacts "package"
$InstallerDir = Join-Path $Artifacts "installer"

Write-Host "=== PCOptimizer Release Build $Version ($Channel) ===" -ForegroundColor Cyan

# Clean
Remove-Item -Recurse -Force $PublishApp, $PublishUpdater, $PackageDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $PublishApp, $PublishUpdater, $PackageDir, $InstallerDir | Out-Null

# 1. Publish main app (framework-dependent, smaller)
Write-Host "`n[1/6] Publishing main application..."
dotnet publish UltimateWindowsOptimizer.App/UltimateWindowsOptimizer.App.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $PublishApp `
    -p:Version=$Version `
    -p:AssemblyName=PCOptimizer `
    -p:PublishSingleFile=false

# 2. Publish updater
Write-Host "`n[2/6] Publishing updater..."
dotnet publish PCOptimizer.Updater/PCOptimizer.Updater.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $PublishUpdater `
    -p:Version=$Version `
    -p:AssemblyName=PCOptimizer.Updater

# Copy updater next to main app for the package
Copy-Item (Join-Path $PublishUpdater "PCOptimizer.Updater.exe") $PublishApp -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $PublishUpdater "PCOptimizer.Updater.dll") $PublishApp -Force -ErrorAction SilentlyContinue
Get-ChildItem $PublishUpdater -Filter "*.dll" | Copy-Item -Destination $PublishApp -Force

# 3. Create zip package
Write-Host "`n[3/6] Creating update package..."
$ZipPath = Join-Path $PackageDir "PCOptimizer-$Version.zip"
if (Test-Path $ZipPath) { Remove-Item $ZipPath }
Compress-Archive -Path (Join-Path $PublishApp "*") -DestinationPath $ZipPath -CompressionLevel Optimal

# 4. SHA-256
Write-Host "`n[4/6] Computing checksums..."
function Get-Sha256($path) {
    $hash = Get-FileHash -Path $path -Algorithm SHA256
    return $hash.Hash.ToLowerInvariant()
}
$ZipSha = Get-Sha256 $ZipPath
$ShaFile = Join-Path $PackageDir "SHA256SUMS.txt"
"$(Get-Sha256 $ZipPath)  PCOptimizer-$Version.zip" | Set-Content $ShaFile

# Optional code signing
if ($SigningCertPath -and (Test-Path $SigningCertPath)) {
    Write-Host "Signing binaries..."
    $signtool = Get-Command signtool -ErrorAction SilentlyContinue
    if ($signtool) {
        $exes = Get-ChildItem $PublishApp -Filter "*.exe"
        foreach ($exe in $exes) {
            & signtool sign /f $SigningCertPath /p $SigningCertPassword /tr http://timestamp.digicert.com /td sha256 /fd sha256 $exe.FullName
        }
    } else {
        Write-Host "signtool not found – skipping signing (see SIGNING.md)" -ForegroundColor Yellow
    }
} else {
    Write-Host "No signing certificate configured – skipping (see SIGNING.md)" -ForegroundColor Yellow
}

# 5. Generate latest.json manifest
Write-Host "`n[5/6] Generating update manifest..."
$manifestName = switch ($Channel) {
    "beta"    { "latest-beta.json" }
    "nightly" { "latest-nightly.json" }
    default   { "latest.json" }
}
$manifest = @{
    version                = $Version
    channel                = $Channel
    releaseDate            = (Get-Date).ToUniversalTime().ToString("o")
    downloadUrl            = "https://github.com/YourOrg/PCOptimizer/releases/download/v$Version/PCOptimizer-$Version.zip"
    sha256                 = $ZipSha
    signature              = $null
    releaseNotes           = "See GitHub Releases for full notes."
    minimumWindowsVersion  = "10.0.19041"
    mandatory              = $false
    fileSizeBytes          = (Get-Item $ZipPath).Length
    installerUrl           = "https://github.com/YourOrg/PCOptimizer/releases/download/v$Version/PCOptimizerSetup.exe"
    installerSha256        = $null
} | ConvertTo-Json -Depth 5

$manifestPath = Join-Path $PackageDir $manifestName
$manifest | Set-Content $manifestPath -Encoding UTF8
Write-Host "Manifest: $manifestPath"
Write-Host "SHA256:   $ZipSha"

# 6. Installer (optional – requires Inno Setup)
Write-Host "`n[6/6] Installer..."
if (-not $SkipInstaller) {
    $iscc = Get-Command iscc -ErrorAction SilentlyContinue
    if (-not $iscc) {
        $isccPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
        if (Test-Path $isccPath) { $iscc = $isccPath }
    }
    if ($iscc) {
        # Patch version into iss
        $iss = Join-Path $Root "installer\PCOptimizer.iss"
        $issContent = Get-Content $iss -Raw
        $issContent = $issContent -replace '#define MyAppVersion "[\d.]+"', "#define MyAppVersion `"$Version`""
        $tempIss = Join-Path $env:TEMP "PCOptimizer_build.iss"
        $issContent | Set-Content $tempIss
        & $iscc $tempIss
        Write-Host "Installer built."
    } else {
        Write-Host "Inno Setup (ISCC) not found – skipping installer. Install from https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    }
} else {
    Write-Host "Skipped (-SkipInstaller)."
}

Write-Host "`n=== Build complete ===" -ForegroundColor Green
Write-Host "Package:  $ZipPath"
Write-Host "Checksum: $ShaFile"
Write-Host "Manifest: $manifestPath"
