# Docker Build Script for Smart CV Filter
# Usage: .\scripts\docker-build.ps1 [-Clean] [-Push] [-Help]

param(
    [switch]$Clean,
    [switch]$Push,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
Smart CV Filter - Docker Build Script

Usage:
    .\scripts\docker-build.ps1              Build all images
    .\scripts\docker-build.ps1 -Clean      Remove existing images before building
    .\scripts\docker-build.ps1 -Push        Build and push to registry (requires registry config)
    .\scripts\docker-build.ps1 -Help        Show this help message

Examples:
    .\scripts\docker-build.ps1
    .\scripts\docker-build.ps1 -Clean
"@
    exit 0
}

Write-Host "Building Smart CV Filter Docker images..." -ForegroundColor Green

# Change to project root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

# Clean existing images if requested
if ($Clean) {
    Write-Host "Cleaning existing images..." -ForegroundColor Yellow
    docker-compose down --rmi all 2>$null
    docker system prune -f
}

# Build images
Write-Host "Building Docker images..." -ForegroundColor Cyan
try {
    docker-compose build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBuild completed successfully!" -ForegroundColor Green
        Write-Host "`nAvailable images:" -ForegroundColor Cyan
        docker images | Select-String "smart-cv-filter"
    } else {
        Write-Host "`nBuild failed!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "`nError during build: $_" -ForegroundColor Red
    exit 1
}

# Push images if requested
if ($Push) {
    Write-Host "`nPushing images to registry..." -ForegroundColor Yellow
    docker-compose push
}

Write-Host "`nDone!" -ForegroundColor Green

