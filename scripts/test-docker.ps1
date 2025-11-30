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
$dockerOutput = docker --version 2>&1
if ($?) {
    Write-Host "   [OK] Docker installed: $dockerOutput" -ForegroundColor Green
} else {
    Write-Host "   [ERROR] Docker not found. Please install Docker Desktop." -ForegroundColor Red
    $errors++
}

# Check Docker Compose
Write-Host "2. Checking Docker Compose..." -ForegroundColor Cyan
$composeOutput = docker-compose --version 2>&1
if ($?) {
    Write-Host "   [OK] Docker Compose installed: $composeOutput" -ForegroundColor Green
} else {
    Write-Host "   [ERROR] Docker Compose not found." -ForegroundColor Red
    $errors++
}

# Check Docker daemon
Write-Host "3. Checking Docker daemon..." -ForegroundColor Cyan
docker info 2>&1 | Out-Null
if ($?) {
    Write-Host "   [OK] Docker daemon is running" -ForegroundColor Green
} else {
    Write-Host "   [ERROR] Docker daemon is not running. Please start Docker Desktop." -ForegroundColor Red
    $errors++
}

# Check docker-compose.yml
Write-Host "4. Checking docker-compose.yml..." -ForegroundColor Cyan
if (Test-Path "docker-compose.yml") {
    Write-Host "   [OK] docker-compose.yml found" -ForegroundColor Green
} else {
    Write-Host "   [ERROR] docker-compose.yml not found" -ForegroundColor Red
    $errors++
}

# Check .env file
Write-Host "5. Checking .env file..." -ForegroundColor Cyan
if (Test-Path ".env") {
    Write-Host "   [OK] .env file found" -ForegroundColor Green
} else {
    Write-Host "   [WARNING] .env file not found (will use defaults)" -ForegroundColor Yellow
    if (Test-Path ".env.example") {
        Write-Host "   [INFO] You can copy .env.example to .env" -ForegroundColor Yellow
    }
}

# Check Dockerfiles
Write-Host "6. Checking Dockerfiles..." -ForegroundColor Cyan
$backendDockerfile = "src/backend/SmartCVFilter.API/Dockerfile"
$frontendDockerfile = "src/frontend/SmartCVFilter.Web/Dockerfile"

if (Test-Path $backendDockerfile) {
    Write-Host "   [OK] Backend Dockerfile found" -ForegroundColor Green
} else {
    Write-Host "   [ERROR] Backend Dockerfile not found" -ForegroundColor Red
    $errors++
}

if (Test-Path $frontendDockerfile) {
    Write-Host "   [OK] Frontend Dockerfile found" -ForegroundColor Green
} else {
    Write-Host "   [ERROR] Frontend Dockerfile not found" -ForegroundColor Red
    $errors++
}

# Check port availability
Write-Host "7. Checking port availability..." -ForegroundColor Cyan
$ports = @(3000, 5000, 5432)
foreach ($port in $ports) {
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        Write-Host "   [WARNING] Port $port is already in use" -ForegroundColor Yellow
    } else {
        Write-Host "   [OK] Port $port is available" -ForegroundColor Green
    }
}

# Validate docker-compose.yml syntax
Write-Host "8. Validating docker-compose.yml syntax..." -ForegroundColor Cyan
$composeOutput = docker-compose config 2>&1
$composeExitCode = $LASTEXITCODE
if ($composeExitCode -eq 0) {
    Write-Host "   [OK] docker-compose.yml syntax is valid" -ForegroundColor Green
    # Check for warnings
    if ($composeOutput -match "warning|obsolete") {
        Write-Host "   [WARNING] docker-compose.yml has deprecation warnings (non-critical)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   [ERROR] docker-compose.yml syntax error" -ForegroundColor Red
    Write-Host "   Error details: $composeOutput" -ForegroundColor Red
    $errors++
}

Write-Host ""
if ($errors -eq 0) {
    Write-Host "All checks passed! You can run: .\scripts\docker-build.ps1" -ForegroundColor Green
} else {
    Write-Host "$errors error(s) found. Please fix them before proceeding." -ForegroundColor Red
    exit 1
}
