# Docker Test Script for Smart CV Filter
# Tests Docker configuration and connectivity

Write-Host "Testing Smart CV Filter Docker Configuration..." -ForegroundColor Green
Write-Host ""

# Change to project root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

$errors = 0

# Check Docker installation
Write-Host "1. Checking Docker installation..." -ForegroundColor Cyan
try {
    $dockerVersion = docker --version
    Write-Host "   ✓ Docker installed: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Docker not found. Please install Docker Desktop." -ForegroundColor Red
    $errors++
}

# Check Docker Compose
Write-Host "2. Checking Docker Compose..." -ForegroundColor Cyan
try {
    $composeVersion = docker-compose --version
    Write-Host "   ✓ Docker Compose installed: $composeVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Docker Compose not found." -ForegroundColor Red
    $errors++
}

# Check Docker daemon
Write-Host "3. Checking Docker daemon..." -ForegroundColor Cyan
try {
    docker info | Out-Null
    Write-Host "   ✓ Docker daemon is running" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Docker daemon is not running. Please start Docker Desktop." -ForegroundColor Red
    $errors++
}

# Check docker-compose.yml
Write-Host "4. Checking docker-compose.yml..." -ForegroundColor Cyan
if (Test-Path "docker-compose.yml") {
    Write-Host "   ✓ docker-compose.yml found" -ForegroundColor Green
} else {
    Write-Host "   ✗ docker-compose.yml not found" -ForegroundColor Red
    $errors++
}

# Check .env file
Write-Host "5. Checking .env file..." -ForegroundColor Cyan
if (Test-Path ".env") {
    Write-Host "   ✓ .env file found" -ForegroundColor Green
} else {
    Write-Host "   ⚠ .env file not found (will use defaults)" -ForegroundColor Yellow
    if (Test-Path ".env.example") {
        Write-Host "   ℹ You can copy .env.example to .env" -ForegroundColor Yellow
    }
}

# Check Dockerfiles
Write-Host "6. Checking Dockerfiles..." -ForegroundColor Cyan
$backendDockerfile = "src/backend/SmartCVFilter.API/Dockerfile"
$frontendDockerfile = "src/frontend/SmartCVFilter.Web/Dockerfile"

if (Test-Path $backendDockerfile) {
    Write-Host "   ✓ Backend Dockerfile found" -ForegroundColor Green
} else {
    Write-Host "   ✗ Backend Dockerfile not found" -ForegroundColor Red
    $errors++
}

if (Test-Path $frontendDockerfile) {
    Write-Host "   ✓ Frontend Dockerfile found" -ForegroundColor Green
} else {
    Write-Host "   ✗ Frontend Dockerfile not found" -ForegroundColor Red
    $errors++
}

# Check port availability
Write-Host "7. Checking port availability..." -ForegroundColor Cyan
$ports = @(3000, 5000, 5050, 5432)
foreach ($port in $ports) {
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        Write-Host "   ⚠ Port $port is already in use" -ForegroundColor Yellow
    } else {
        Write-Host "   ✓ Port $port is available" -ForegroundColor Green
    }
}

# Validate docker-compose.yml syntax
Write-Host "8. Validating docker-compose.yml syntax..." -ForegroundColor Cyan
try {
    docker-compose config | Out-Null
    Write-Host "   ✓ docker-compose.yml syntax is valid" -ForegroundColor Green
} catch {
    Write-Host "   ✗ docker-compose.yml syntax error" -ForegroundColor Red
    $errors++
}

Write-Host ""
if ($errors -eq 0) {
    Write-Host "All checks passed! You can run: .\scripts\docker-build.ps1" -ForegroundColor Green
} else {
    Write-Host "$errors error(s) found. Please fix them before proceeding." -ForegroundColor Red
    exit 1
}

