#!/usr/bin/env pwsh

# Build script for optimized chess engine release

param(
    [string]$Runtime = "win-x64",
    [switch]$Clean = $false
)

$currentDir = Get-Location
Write-Host "Building optimized chess engine for $Runtime..." -ForegroundColor Green
Write-Host "Working directory: $currentDir" -ForegroundColor Gray

if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean --configuration Release
    Remove-Item -Path "ChessEngine\bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "ChessEngine\obj\Release" -Recurse -Force -ErrorAction SilentlyContinue
}

# Build and publish with Native AOT
Write-Host "Publishing with Native AOT compilation..." -ForegroundColor Yellow
$projectPath = ".\ChessEngine\ChessEngine.csproj"
$publishPath = "ChessEngine\bin\Release\net9.0\$Runtime\publish"

Write-Host "Project path: $projectPath" -ForegroundColor Gray

dotnet publish $projectPath `
    --configuration Release `
    --runtime $Runtime `
    --self-contained `
    --verbosity minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host "Executable location: $publishPath\ChessEngine.exe" -ForegroundColor Cyan
    
    # Show file size
    $exePath = "$publishPath\ChessEngine.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length / 1MB
        Write-Host "Executable size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
    }
    
    # Copy to engines directory
    $enginesDir = "C:\dev\engines\slopfish"
    Write-Host "Copying executable to $enginesDir..." -ForegroundColor Yellow
    
    if (-not (Test-Path $enginesDir)) {
        New-Item -ItemType Directory -Path $enginesDir -Force | Out-Null
        Write-Host "Created directory: $enginesDir" -ForegroundColor Green
    }
    
    Copy-Item $exePath $enginesDir -Force
    Write-Host "Executable copied to: $enginesDir\ChessEngine.exe" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
