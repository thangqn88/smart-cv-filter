# Docker Run Script for Smart CV Filter
# Usage: .\scripts\docker-run.ps1 [-Foreground] [-Clean] [-Logs] [-Stop] [-Restart] [-Help]

param(
    [switch]$Foreground,
    [switch]$Clean,
    [switch]$Logs,
    [switch]$Stop,
    [switch]$Restart,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
Smart CV Filter - Docker Run Script

Usage:
    .\scripts\docker-run.ps1                Start application (detached mode)
    .\scripts\docker-run.ps1 -Foreground    Start in foreground (see logs)
    .\scripts\docker-run.ps1 -Clean         Clean start (remove containers and volumes)
    .\scripts\docker-run.ps1 -Logs          Start and follow logs
    .\scripts\docker-run.ps1 -Stop          Stop application
    .\scripts\docker-run.ps1 -Restart       Restart application
    .\scripts\docker-run.ps1 -Help          Show this help message

Examples:
    .\scripts\docker-run.ps1
    .\scripts\docker-run.ps1 -Foreground
    .\scripts\docker-run.ps1 -Clean
"@
    exit 0
}

# Change to project root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "Warning: .env file not found. Creating from .env.example..." -ForegroundColor Yellow
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Host "Please update .env file with your configuration before running." -ForegroundColor Yellow
    } else {
        Write-Host "Error: .env.example file not found. Please create .env file manually." -ForegroundColor Red
        exit 1
    }
}

# Stop containers if requested
if ($Stop) {
    Write-Host "Stopping Smart CV Filter containers..." -ForegroundColor Yellow
    docker-compose down
    Write-Host "Containers stopped." -ForegroundColor Green
    exit 0
}

# Clean start if requested
if ($Clean) {
    Write-Host "Cleaning containers and volumes..." -ForegroundColor Yellow
    docker-compose down -v
    Write-Host "Cleanup completed." -ForegroundColor Green
}

# Start containers
Write-Host "Starting Smart CV Filter application..." -ForegroundColor Green

try {
    if ($Foreground) {
        # Start in foreground
        docker-compose up
    } elseif ($Logs) {
        # Start detached and follow logs
        docker-compose up -d
        Start-Sleep -Seconds 5
        docker-compose logs -f
    } else {
        # Start detached
        docker-compose up -d
        
        Write-Host "`nApplication is starting..." -ForegroundColor Cyan
        Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        # Check service status
        Write-Host "`nService Status:" -ForegroundColor Cyan
        docker-compose ps
        
        Write-Host "`nApplication URLs:" -ForegroundColor Green
        Write-Host "  Frontend:    http://localhost:3000" -ForegroundColor White
        Write-Host "  Backend API: http://localhost:5000" -ForegroundColor White
        Write-Host "  Swagger:     http://localhost:5000/swagger" -ForegroundColor White
        Write-Host "  pgAdmin:     http://localhost:5050" -ForegroundColor White
        
        Write-Host "`nTo view logs: docker-compose logs -f" -ForegroundColor Yellow
        Write-Host "To stop:       .\scripts\docker-run.ps1 -Stop" -ForegroundColor Yellow
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nError starting containers!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "`nError: $_" -ForegroundColor Red
    exit 1
}

# Restart if requested
if ($Restart) {
    Write-Host "Restarting containers..." -ForegroundColor Yellow
    docker-compose restart
    Write-Host "Containers restarted." -ForegroundColor Green
}

