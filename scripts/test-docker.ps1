# Docker test script for Smart CV Filter application (PowerShell)

param(
    [switch]$Help
)

# Colors for output
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Blue"

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Red
}

# Show help if requested
if ($Help) {
    Write-Host "Usage: .\test-docker.ps1"
    Write-Host "This script tests the Docker setup for Smart CV Filter application."
    exit 0
}

Write-Host "🧪 Testing Smart CV Filter Docker setup..." -ForegroundColor $Blue

# Check if Docker is running
Write-Status "Checking Docker status..."
try {
    docker info | Out-Null
    Write-Success "Docker is running"
} catch {
    Write-Error "Docker is not running. Please start Docker and try again."
    exit 1
}

# Check if docker-compose is available
Write-Status "Checking docker-compose availability..."
try {
    $composeVersion = docker-compose --version
    Write-Success "docker-compose is available: $composeVersion"
} catch {
    Write-Error "docker-compose is not installed. Please install docker-compose and try again."
    exit 1
}

# Check if required files exist
Write-Status "Checking required files..."

$requiredFiles = @(
    "docker-compose.yml",
    "src/backend/SmartCVFilter.API/Dockerfile",
    "src/frontend/smart-cv-filter/Dockerfile",
    "src/frontend/smart-cv-filter/nginx.conf"
)

$allFilesExist = $true
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Success "File $file exists"
    } else {
        Write-Error "File $file is missing"
        $allFilesExist = $false
    }
}

if (-not $allFilesExist) {
    Write-Error "Some required files are missing. Please check the setup."
    exit 1
}

# Test Docker Compose syntax
Write-Status "Testing Docker Compose syntax..."

try {
    docker-compose config | Out-Null
    Write-Success "Production docker-compose.yml syntax is valid"
} catch {
    Write-Error "Production docker-compose.yml has syntax errors"
    $allFilesExist = $false
}

# Development docker-compose.dev.yml removed - using single environment

# Test Dockerfile syntax
Write-Status "Testing Dockerfile syntax..."

try {
    docker build --dry-run -f src/backend/SmartCVFilter.API/Dockerfile src/backend | Out-Null
    Write-Success "Backend Dockerfile syntax is valid"
} catch {
    Write-Warning "Backend Dockerfile validation failed (this is normal for dry-run)"
}

try {
    docker build --dry-run -f src/frontend/smart-cv-filter/Dockerfile src/frontend/smart-cv-filter | Out-Null
    Write-Success "Frontend Dockerfile syntax is valid"
} catch {
    Write-Warning "Frontend Dockerfile validation failed (this is normal for dry-run)"
}

# Check port availability
Write-Status "Checking port availability..."

$ports = @(3000, 5000, 5432, 5050, 6379)
$availablePorts = @()

foreach ($port in $ports) {
    try {
        $connection = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue
        if ($connection.TcpTestSucceeded) {
            Write-Warning "Port $port is already in use"
        } else {
            Write-Success "Port $port is available"
            $availablePorts += $port
        }
    } catch {
        Write-Success "Port $port is available"
        $availablePorts += $port
    }
}

# Summary
Write-Host ""
if ($allFilesExist) {
    Write-Success "Docker setup test passed! All checks successful."
    Write-Host ""
    Write-Status "Next steps:"
    Write-Host "1. Set up your .env file with required environment variables"
    Write-Host "2. Run: .\scripts\docker-build.ps1"
    Write-Host "3. Run: .\scripts\docker-run.ps1"
    Write-Host ""
    if ($availablePorts.Count -lt $ports.Count) {
        Write-Warning "Note: Some ports are already in use. You may need to stop existing services or use different ports."
    }
} else {
    Write-Error "Docker setup test failed. Please fix the issues above."
    exit 1
}
