#!/usr/bin/env pwsh
# quick-test.ps1 - Quick local validation before committing

Write-Host "`n?? Quick Build & Test" -ForegroundColor Cyan

# Quick validation
Write-Host "`n? Building .NET 8 API..." -ForegroundColor Yellow
dotnet build API_Service/API_Service.csproj --configuration Release --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "? API build successful" -ForegroundColor Green
} else {
    Write-Host "? API build failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n? Running tests..." -ForegroundColor Yellow
dotnet test TestProject/TestProject.csproj --configuration Release --verbosity quiet --no-build

if ($LASTEXITCODE -eq 0) {
    Write-Host "? All tests passed" -ForegroundColor Green
} else {
    Write-Host "? Tests failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n? Quick validation passed! Safe to commit." -ForegroundColor Green
exit 0
