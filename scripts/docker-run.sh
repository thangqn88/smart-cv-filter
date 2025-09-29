#!/bin/bash

# Docker run script for Smart CV Filter application

set -e

echo "🚀 Starting Smart CV Filter application..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Parse command line arguments
DETACHED=true
CLEAN_START=false
FOLLOW_LOGS=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --foreground)
            DETACHED=false
            shift
            ;;
        --clean)
            CLEAN_START=true
            shift
            ;;
        --logs)
            FOLLOW_LOGS=true
            shift
            ;;
        --stop)
            print_status "Stopping Smart CV Filter application..."
            docker-compose down
            print_success "Application stopped successfully!"
            exit 0
            ;;
        --restart)
            print_status "Restarting Smart CV Filter application..."
            docker-compose down
            sleep 2
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --foreground  Run in foreground (not detached)"
            echo "  --clean       Clean start (remove existing containers and volumes)"
            echo "  --logs        Follow logs after starting"
            echo "  --stop        Stop the application"
            echo "  --restart     Restart the application"
            echo "  -h, --help    Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    print_error "docker-compose is not installed. Please install docker-compose and try again."
    exit 1
fi

# Set compose file
COMPOSE_FILE="docker-compose.yml"
print_status "Starting Smart CV Filter application..."

# Clean start if requested
if [ "$CLEAN_START" = true ]; then
    print_status "Performing clean start..."
    docker-compose -f $COMPOSE_FILE down --volumes --remove-orphans
    docker system prune -f
fi

# Start services
print_status "Starting services..."

if [ "$DETACHED" = true ]; then
    docker-compose -f $COMPOSE_FILE up -d
else
    docker-compose -f $COMPOSE_FILE up
fi

if [ $? -eq 0 ]; then
    print_success "Application started successfully!"
    
    # Wait for services to be healthy
    print_status "Waiting for services to be ready..."
    sleep 10
    
    # Show service status
    print_status "Service status:"
    docker-compose -f $COMPOSE_FILE ps
    
    # Show access URLs
    echo ""
    print_success "🎉 Smart CV Filter is now running!"
    echo ""
    echo "📱 Frontend: http://localhost:3000"
    echo "🔧 Backend API: http://localhost:5000"
    echo "🗄️  PostgreSQL: localhost:5432"
    echo "📊 pgAdmin: http://localhost:5050 (admin@smartcv.com / admin123)"
    echo "🔴 Redis: localhost:6379"
    echo ""
    
    if [ "$FOLLOW_LOGS" = true ]; then
        print_status "Following logs (Press Ctrl+C to stop)..."
        docker-compose -f $COMPOSE_FILE logs -f
    else
        print_status "To view logs, run: docker-compose -f $COMPOSE_FILE logs -f"
        print_status "To stop the application, run: $0 --stop"
    fi
else
    print_error "Failed to start the application"
    exit 1
fi
