# Docker build script for Smart CV Filter application (PowerShell)

param(
    [switch]$Clean,
    [switch]$Push,
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
    Write-Host "Usage: .\docker-build.ps1 [OPTIONS]"
    Write-Host "Options:"
    Write-Host "  -Clean    Clean build (remove existing images)"
    Write-Host "  -Push     Push images to registry after build"
    Write-Host "  -Help     Show this help message"
    exit 0
}

Write-Host "🐳 Building Smart CV Filter Docker containers..." -ForegroundColor $Blue

# Check if Docker is running
try {
    docker info | Out-Null
} catch {
    Write-Error "Docker is not running. Please start Docker and try again."
    exit 1
}

# Check if docker-compose is available
try {
    docker-compose --version | Out-Null
} catch {
    Write-Error "docker-compose is not installed. Please install docker-compose and try again."
    exit 1
}

# Clean build if requested
if ($Clean) {
    Write-Status "Cleaning existing images..."
    try {
        docker-compose down --rmi all --volumes --remove-orphans 2>$null
        docker system prune -f
    } catch {
        # Ignore errors during cleanup
    }
}

# Set compose file
$ComposeFile = "docker-compose.yml"
Write-Status "Building Smart CV Filter application..."

# Build images
Write-Status "Building Docker images..."
try {
    docker-compose -f $ComposeFile build --no-cache
    Write-Success "Docker images built successfully!"
} catch {
    Write-Error "Failed to build Docker images"
    exit 1
}

# Push images if requested
if ($Push) {
    Write-Status "Pushing images to registry..."
    try {
        docker-compose -f $ComposeFile push
        Write-Success "Images pushed successfully!"
    } catch {
        Write-Error "Failed to push images"
        exit 1
    }
}

Write-Success "Build completed successfully!"
Write-Status "To start the application, run: .\scripts\docker-run.ps1"
