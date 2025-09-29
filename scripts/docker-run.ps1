# Docker run script for Smart CV Filter application (PowerShell)

param(
    [switch]$Foreground,
    [switch]$Clean,
    [switch]$Logs,
    [switch]$Stop,
    [switch]$Restart,
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
    Write-Host "Usage: .\docker-run.ps1 [OPTIONS]"
    Write-Host "Options:"
    Write-Host "  -Foreground  Run in foreground (not detached)"
    Write-Host "  -Clean       Clean start (remove existing containers and volumes)"
    Write-Host "  -Logs        Follow logs after starting"
    Write-Host "  -Stop        Stop the application"
    Write-Host "  -Restart     Restart the application"
    Write-Host "  -Help        Show this help message"
    exit 0
}

Write-Host "🚀 Starting Smart CV Filter application..." -ForegroundColor $Blue

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

# Set compose file
$ComposeFile = "docker-compose.yml"
Write-Status "Starting Smart CV Filter application..."

# Handle stop command
if ($Stop) {
    Write-Status "Stopping Smart CV Filter application..."
    try {
        docker-compose -f $ComposeFile down
        Write-Success "Application stopped successfully!"
    } catch {
        Write-Error "Failed to stop application"
        exit 1
    }
    exit 0
}

# Handle restart command
if ($Restart) {
    Write-Status "Restarting Smart CV Filter application..."
    try {
        docker-compose -f $ComposeFile down
        Start-Sleep -Seconds 2
    } catch {
        # Ignore errors during stop
    }
}

# Clean start if requested
if ($Clean) {
    Write-Status "Performing clean start..."
    try {
        docker-compose -f $ComposeFile down --volumes --remove-orphans
        docker system prune -f
    } catch {
        # Ignore errors during cleanup
    }
}

# Start services
Write-Status "Starting services..."

try {
    if ($Foreground) {
        docker-compose -f $ComposeFile up
    } else {
        docker-compose -f $ComposeFile up -d
    }
    
    Write-Success "Application started successfully!"
    
    # Wait for services to be healthy
    Write-Status "Waiting for services to be ready..."
    Start-Sleep -Seconds 10
    
    # Show service status
    Write-Status "Service status:"
    docker-compose -f $ComposeFile ps
    
    # Show access URLs
    Write-Host ""
    Write-Success "🎉 Smart CV Filter is now running!"
    Write-Host ""
    Write-Host "📱 Frontend: http://localhost:3000" -ForegroundColor $Green
    Write-Host "🔧 Backend API: http://localhost:5000" -ForegroundColor $Green
    Write-Host "🗄️  PostgreSQL: localhost:5432" -ForegroundColor $Green
    Write-Host "📊 pgAdmin: http://localhost:5050 (admin@smartcv.com / admin123)" -ForegroundColor $Green
    Write-Host "🔴 Redis: localhost:6379" -ForegroundColor $Green
    Write-Host ""
    
    if ($Logs) {
        Write-Status "Following logs (Press Ctrl+C to stop)..."
        docker-compose -f $ComposeFile logs -f
    } else {
        Write-Status "To view logs, run: docker-compose -f $ComposeFile logs -f"
        Write-Status "To stop the application, run: .\scripts\docker-run.ps1 -Stop"
    }
} catch {
    Write-Error "Failed to start the application"
    exit 1
}
