#!/usr/bin/env pwsh
# run-local-ci.ps1 - Comprehensive local CI/CD test script
# This simulates the GitHub Actions workflow locally on Windows

param(
    [switch]$SkipFramework,
    [switch]$SkipCore,
    [switch]$SkipTests,
    [switch]$SkipClean,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$VerbosePreference = if ($Verbose) { "Continue" } else { "SilentlyContinue" }

# Colors for output
function Write-Section($message) {
    Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "?  $message" -ForegroundColor Cyan
    Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
}

function Write-Step($message) {
    Write-Host "? $message" -ForegroundColor Yellow
}

function Write-Success($message) {
    Write-Host "? $message" -ForegroundColor Green
}

function Write-Failure($message) {
    Write-Host "? $message" -ForegroundColor Red
}

# Start
Write-Section "Local CI/CD Simulation"

# Check prerequisites
Write-Section "Checking Prerequisites"

$prerequisites = @{
    "NuGet CLI" = "nuget"
    "MSBuild" = "msbuild"
    ".NET SDK" = "dotnet"
}

$missingTools = @()
foreach ($tool in $prerequisites.GetEnumerator()) {
    Write-Step "Checking for $($tool.Key)..."
    if (Get-Command $tool.Value -ErrorAction SilentlyContinue) {
        $version = & $tool.Value --version 2>$null | Select-Object -First 1
        Write-Success "$($tool.Key) found: $version"
    } else {
        Write-Failure "$($tool.Key) not found"
        $missingTools += $tool.Key
    }
}

if ($missingTools.Count -gt 0) {
    Write-Host "`nMissing tools:" -ForegroundColor Red
    $missingTools | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "`nPlease install missing tools and try again." -ForegroundColor Yellow
    exit 1
}

Write-Success "All prerequisites satisfied"

# Track timing
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# Clean previous builds
if (-not $SkipClean) {
    Write-Section "Cleaning Previous Builds"
    Write-Step "Cleaning bin and obj directories..."
    
    Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | ForEach-Object {
        Write-Verbose "Removing $_"
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    Write-Success "Clean complete"
}

# .NET Framework Projects
if (-not $SkipFramework) {
    Write-Section ".NET Framework Projects"
    
    # Restore packages
    Write-Step "Restoring NuGet packages..."
    try {
        nuget restore DataContext/DataContext.csproj -PackagesDirectory packages -NonInteractive | Out-Null
        nuget restore web/web.csproj -PackagesDirectory packages -NonInteractive | Out-Null
        Write-Success "Package restore complete"
    }
    catch {
        Write-Failure "Package restore failed: $_"
        exit 1
    }
    
    # Build DataContext
    Write-Step "Building DataContext (.NET Framework 4.8)..."
    try {
        $msbuildArgs = @(
            "DataContext/DataContext.csproj",
            "/p:Configuration=Release",
            "/p:Platform=AnyCPU",
            "/v:minimal",
            "/nologo"
        )
        msbuild @msbuildArgs
        if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
        Write-Success "DataContext build successful"
    }
    catch {
        Write-Failure "DataContext build failed: $_"
        exit 1
    }
    
    # Build Web
    Write-Step "Building Web MVC (.NET Framework 4.8.1)..."
    try {
        $msbuildArgs = @(
            "web/web.csproj",
            "/p:Configuration=Release",
            "/p:Platform=AnyCPU",
            "/v:minimal",
            "/nologo"
        )
        msbuild @msbuildArgs
        if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
        Write-Success "Web MVC build successful"
    }
    catch {
        Write-Failure "Web build failed: $_"
        exit 1
    }
}

# .NET 8 Projects
if (-not $SkipCore) {
    Write-Section ".NET 8 Projects"
    
    # Restore API Service
    Write-Step "Restoring API Service packages..."
    try {
        dotnet restore API_Service/API_Service.csproj --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "Restore failed with exit code $LASTEXITCODE" }
        Write-Success "Package restore complete"
    }
    catch {
        Write-Failure "Package restore failed: $_"
        exit 1
    }
    
    # Build API Service
    Write-Step "Building API Service (.NET 8)..."
    try {
        dotnet build API_Service/API_Service.csproj --configuration Release --no-restore --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
        Write-Success "API Service build successful"
    }
    catch {
        Write-Failure "API Service build failed: $_"
        exit 1
    }
    
    # Restore Test Project
    Write-Step "Restoring Test Project packages..."
    try {
        dotnet restore TestProject/TestProject.csproj --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "Restore failed with exit code $LASTEXITCODE" }
        Write-Success "Package restore complete"
    }
    catch {
        Write-Failure "Package restore failed: $_"
        exit 1
    }
}

# Run Tests
if (-not $SkipTests) {
    Write-Section "Running Unit Tests"
    
    Write-Step "Executing tests with coverage..."
    try {
        dotnet test TestProject/TestProject.csproj `
            --configuration Release `
            --logger "console;verbosity=normal" `
            --collect:"XPlat Code Coverage"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Some tests failed"
            exit 1
        }
        Write-Success "All tests passed"
    }
    catch {
        Write-Failure "Test execution failed: $_"
        exit 1
    }
}

# Security Scan
Write-Section "Security Vulnerability Check"

Write-Step "Scanning for vulnerable packages..."
$vulnerableFound = $false

Write-Host "`nAPI Service:" -ForegroundColor Cyan
$apiScanOutput = dotnet list API_Service/API_Service.csproj package --vulnerable --include-transitive 2>&1
Write-Host $apiScanOutput

if ($apiScanOutput -match "has the following vulnerable packages") {
    $vulnerableFound = $true
}

Write-Host "`nTest Project:" -ForegroundColor Cyan
$testScanOutput = dotnet list TestProject/TestProject.csproj package --vulnerable --include-transitive 2>&1
Write-Host $testScanOutput

if ($testScanOutput -match "has the following vulnerable packages") {
    $vulnerableFound = $true
}

if ($vulnerableFound) {
    Write-Host "`n??  Vulnerable packages detected! Please review and update." -ForegroundColor Yellow
} else {
    Write-Success "No vulnerable packages found"
}

# Check for outdated packages
Write-Section "Outdated Packages Check"

Write-Step "Checking for outdated packages..."
Write-Host "`nAPI Service:" -ForegroundColor Cyan
dotnet list API_Service/API_Service.csproj package --outdated

Write-Host "`nTest Project:" -ForegroundColor Cyan
dotnet list TestProject/TestProject.csproj package --outdated

# Summary
$stopwatch.Stop()
$elapsed = $stopwatch.Elapsed

Write-Section "Build Summary"

Write-Host "`n???????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  Build Status: " -NoNewline -ForegroundColor Green
Write-Host "SUCCESS" -ForegroundColor Green -BackgroundColor Black
Write-Host "?  Duration: $($elapsed.Minutes)m $($elapsed.Seconds)s" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????" -ForegroundColor Green

Write-Host "`n? All checks passed! Ready to push to GitHub." -ForegroundColor Green

# Optional: Show build artifacts
if ($Verbose) {
    Write-Host "`nBuild Artifacts:" -ForegroundColor Cyan
    Get-ChildItem -Path . -Include *.dll,*.exe -Recurse | 
        Where-Object { $_.FullName -match "\\bin\\Release\\" } | 
        ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
}

Write-Host ""
exit 0
